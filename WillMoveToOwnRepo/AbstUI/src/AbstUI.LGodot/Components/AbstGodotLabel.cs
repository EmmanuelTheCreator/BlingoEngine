using Godot;
using AbstUI.Styles;
using static Godot.TextServer;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Texts;
using AbstUI.LGodot.Texts;
using AbstUI.LGodot.Primitives;


namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkLabel"/>.
    /// </summary>
    public partial class AbstGodotLabel : Label, IAbstFrameworkLabel, IDisposable
    {
        private readonly IAbstFontManager _fontManager;
        private AMargin _margin = AMargin.Zero;
        private string? _font;
        private AColor _fontColor;

        public AbstGodotLabel(AbstLabel label, IAbstFontManager fontManager)
        {
            _fontManager = fontManager;
            label.Init(this);
            LabelSettings = new LabelSettings();
            AutowrapMode = TextServer.AutowrapMode.Off;
            
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
    
        public bool Visibility { get => Visible; set => Visible = value; }
        public AbstTextAlignment TextAlignment { get => HorizontalAlignment.ToAbst(); set => HorizontalAlignment = value.ToGodot(); }
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

        public new string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        public int FontSize
        {
            get => (int)(LabelSettings?.FontSize ?? 0);
            set
            {
                var ls = LabelSettings ?? new LabelSettings();
                ls.FontSize = value;
                LabelSettings = ls;
            }
        }
        private int _lineHeight;
        public int LineHeight
        {
            get => _lineHeight;
            set
            {
                _lineHeight = value;
                var ls = LabelSettings ?? new LabelSettings();
                var font = ls.Font;
                if (font != null)
                {
                    // Get base line height for current font size
                    float lineHeight = font.GetHeight(ls.FontSize);

                    // Add your desired spacing in pixels
                    float spacingInPixels = value;

                    // Compute scale multiplier
                    float spacingMultiplier = (lineHeight + spacingInPixels) / lineHeight;

                    // Apply to label
                    ls.LineSpacing = spacingMultiplier;
                    LabelSettings = ls;
                }

            }
        }

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

        public AColor FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                var ls = LabelSettings ?? new LabelSettings();
                ls.FontColor = value.ToGodotColor();
                LabelSettings = ls;
            }
        }

        public object FrameworkNode => this;

        public ATextWrapMode WrapMode { get => (ATextWrapMode)(int)AutowrapMode; set => AutowrapMode = (AutowrapMode)(int)value; }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
