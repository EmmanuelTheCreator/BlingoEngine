using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.LGodot.Bitmaps;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkButton"/>.
    /// </summary>
    public partial class AbstGodotButton : Button, IAbstFrameworkButton, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private IAbstTexture2D? _texture;

        private event Action? _pressed;
       
        public object FrameworkNode => this;

        public AbstGodotButton(AbstButton button, IAbstFontManager lingoFontManager)
        {
            button.Init(this);
            Pressed += () => _pressed?.Invoke();
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }

        public bool Visibility { get => Visible; set => Visible = value; }
        
       
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

        public new string Text { get => base.Text; set => base.Text = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        event Action? IAbstFrameworkButton.Pressed
        {
            add => _pressed += value;
            remove => _pressed -= value;
        }

        public IAbstTexture2D? IconTexture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (_texture != null && _texture is AbstGodotTexture2D tex)
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
