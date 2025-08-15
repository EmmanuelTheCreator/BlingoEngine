using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstUIFrameworkGfxButton"/>.
    /// </summary>
    public partial class LingoGodotButton : Button, IAbstUIFrameworkGfxButton, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private IAbstUITexture2D? _texture;

        private event Action? _pressed;
       
        public object FrameworkNode => this;

        public LingoGodotButton(AbstUIGfxButton button, LingoEngine.Styles.ILingoFontManager lingoFontManager)
        {
            button.Init(this);
            Pressed += () => _pressed?.Invoke();
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }

        public bool Visibility { get => Visible; set => Visible = value; }
        
       
        string IAbstUIFrameworkGfxNode.Name { get => Name; set => Name = value; }

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

        public new string Text { get => base.Text; set => base.Text = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        event Action? IAbstUIFrameworkGfxButton.Pressed
        {
            add => _pressed += value;
            remove => _pressed -= value;
        }

        public IAbstUITexture2D? IconTexture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (_texture != null && _texture is LingoGodotTexture2D tex)
                    Icon = tex.Texture;
            }
        }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

        
        
    }
}
