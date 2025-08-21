using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Components.Inputs;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputCombobox"/>.
    /// </summary>
    public partial class AbstGodotInputCombobox : OptionButton, IAbstFrameworkInputCombobox, IHasTextBackgroundBorderColor, IDisposable
    {
        private readonly List<KeyValuePair<string, string>> _items = new();
        private AMargin _margin = AMargin.Zero;
        private Action<string?>? _onChange;
        private AColor _textColor = AbstDefaultColors.InputTextColor;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;

        private event Action? _onValueChanged;

        public AbstGodotInputCombobox(AbstInputCombobox input, IAbstFontManager lingoFontManager, Action<string?>? onChange)
        {
            input.Init(this);
            ItemSelected += idx => _onValueChanged?.Invoke();
            _onChange = onChange;
            if (_onChange != null) ItemSelected += _ => _onChange(SelectedKey);
        }


        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

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
        public object FrameworkNode => this;
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
            int idx = ItemCount;
            base.AddItem(value);
            SetItemMetadata(idx, key);
        }
        public void ClearItems()
        {
            _items.Clear();
            Clear();
        }
        public int SelectedIndex { get => Selected; set => Selected = value; }
        public string? SelectedKey
        {
            get => Selected >= 0 ? (string?)GetItemMetadata(Selected) : null;
            set
            {
                if (value is null) { Selected = -1; return; }
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((string?)GetItemMetadata(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }
        public string? SelectedValue
        {
            get => Selected >= 0 ? GetItemText(Selected) : null;
            set
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (GetItemText(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }

        public string? ItemFont { get; set; }
        public int ItemFontSize { get; set; } = 11;
        public AColor ItemTextColor { get; set; } = AbstDefaultColors.InputTextColor;
        public AColor ItemSelectedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
        public AColor ItemSelectedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public AColor ItemSelectedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public AColor ItemHoverTextColor { get; set; } = AbstDefaultColors.InputTextColor;
        public AColor ItemHoverBackgroundColor { get; set; } = AbstDefaultColors.ListHoverColor;
        public AColor ItemHoverBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public AColor ItemPressedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
        public AColor ItemPressedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public AColor ItemPressedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            if (_onChange != null) ItemSelected -= _ => _onChange(SelectedKey);
            ItemSelected -= idx => _onValueChanged?.Invoke();
            QueueFree();
            base.Dispose();
        }
    }
}
