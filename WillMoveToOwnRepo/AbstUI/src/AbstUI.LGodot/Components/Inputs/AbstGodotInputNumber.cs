using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.LGodot.Primitives;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputNumber"/>.
    /// </summary>
    public partial class AbstGodotInputNumber<TValue> : LineEdit, IAbstFrameworkInputNumber<TValue>, IHasTextBackgroundBorderColor, IDisposable
        where TValue : System.Numerics.INumber<TValue>
    {
        private AMargin _margin = AMargin.Zero;
        private ANumberType _numberType = ANumberType.Float;
        private Action<TValue>? _onChange;
        private readonly IAbstFontManager _fontManager;
        private AColor _textColor = AbstDefaultColors.InputTextColor;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;

        private event Action _onValueChanged;

        public AbstGodotInputNumber(AbstInputNumber<TValue> input, IAbstFontManager fontManager, Action<TValue>? onChange)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            Func<string, (bool IsValid,TValue Value)> valueParse;
            // Switch case to set Min and Max based on type  
            switch (Type.GetTypeCode(typeof(TValue)))
            {
                case TypeCode.Int32:
                    Min = TValue.CreateChecked(int.MinValue);
                    Max = TValue.CreateChecked(int.MaxValue);
                    valueParse = v => int.TryParse(Text, out var newValue) ? (true, TValue.CreateChecked(newValue)) : (false, TValue.Zero);
                    break;
                case TypeCode.Single:
                    Min = TValue.CreateChecked(float.MinValue);
                    Max = TValue.CreateChecked(float.MaxValue);
                    valueParse = v => float.TryParse(Text, out var newValue) ? (true, TValue.CreateChecked(newValue)) : (false, TValue.Zero);
                    break;
                case TypeCode.Double:
                    Min = TValue.CreateChecked(double.MinValue);
                    Max = TValue.CreateChecked(double.MaxValue);
                    valueParse = v => double.TryParse(Text, out var newValue) ? (true, TValue.CreateChecked(newValue)) : (false, TValue.Zero);
                    break;
                default:
                    throw new NotSupportedException($"Type {typeof(TValue)} is not supported.");
            }


            input.Init(this);
            _onValueChanged = () =>
            {
                var parsedValue = valueParse(Text);
                if (!parsedValue.IsValid)
                {
                    if (_value != null)
                    Value = _value;
                    return;
                }
                if (_value == parsedValue.Value) return;
                _value = parsedValue.Value;
                _onChange?.Invoke(Value);
            };
            TextChanged += _ => _onValueChanged.Invoke();
            CustomMinimumSize = new Vector2(2, 2);
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            SizeFlagsVertical = SizeFlags.ShrinkBegin;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        private float _wantedWidth = 10;
        public float Width { get => Size.X;
            set
            {
                _wantedWidth = value;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(value, _wantedHeight);
                
                var test = Size;
                
            } }
        private float _wantedHeight = 10;
        public float Height
        {
            get => Size.Y;
            set
            {
                _wantedHeight = Height;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(_wantedWidth, value);
            }
        }

        public override void _Ready()
        {
            base._Ready();
            CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
            Size = new Vector2(_wantedWidth, _wantedHeight);
            // these are needed in the styling:
            //theme.SetConstant("minimum_height", controlType, 10);
            //theme.SetConstant("minimum_width", controlType, 5);
            //theme.SetConstant("minimum_spaces", controlType, 1);
            //theme.SetConstant("minimum_character_width", controlType, 0);
        }

        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => Editable; set => Editable = value; }

        private TValue _value;
        public TValue Value
        {
            get => _value; set
            {
                _value = value;
                if (_value > Max) _value = Max;
                if (_value < Min) _value = Min;
                Text = _value.ToString();
            }
        }
        public TValue Min { get; set; } 
        public TValue Max { get; set; } 
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
        public ANumberType NumberType
        {
            get => _numberType;
            set
            {
                _numberType = value;
            }
        }

        private string? _font;
        public string? Font
        {
            get => _font;
            set
            {
                _font = value;
                if (string.IsNullOrEmpty(value))
                {
                    RemoveThemeFontOverride("font");
                }
                else
                {
                    var font = _fontManager.Get<FontFile>(value);
                    if (font != null)
                        AddThemeFontOverride("font", font);
                }
            }
        }
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Font? baseFont;
                if (string.IsNullOrEmpty(_font))
                    baseFont = _fontManager.GetDefaultFont<Font>();
                else
                    baseFont = _fontManager.Get<Font>(_font);

                if (baseFont == null)
                    return;

                // Create a FontVariation with size applied through theme variation
                var variation = new FontVariation
                {
                    BaseFont = baseFont
                };

                // Set the size override via theme properties
                var theme = new Theme();
                theme.SetFont("font", "LineEdit", variation);
                theme.SetFontSize("font_size", "LineEdit", _fontSize);
                Theme = theme;
            }
        }
        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public AColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                AddThemeColorOverride("font_color", _textColor.ToGodotColor());
            }
        }

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                AddThemeColorOverride("background_color", _backgroundColor.ToGodotColor());
            }
        }

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                AddThemeColorOverride("border_color", _borderColor.ToGodotColor());
            }
        }


        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }
        public object FrameworkNode => this;
        public new void Dispose()
        {
            TextChanged -= _ => _onValueChanged.Invoke();
            QueueFree();
            base.Dispose();
        }
    }
}
