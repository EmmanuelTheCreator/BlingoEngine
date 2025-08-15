using Godot;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxVerticalLineSeparator"/>.
    /// </summary>
    public partial class LingoGodotVerticalLineSeparator : Control, ILingoFrameworkGfxVerticalLineSeparator, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private Color _lightColor;
        private Color _darkColor;

        public LingoGodotVerticalLineSeparator(LingoGfxVerticalLineSeparator separator)
        {
            separator.Init(this);
            _lightColor = new Color(1, 1, 1);
            _darkColor = new Color("#a0a0a0");
        }


        public override void _Draw()
        {
            DrawLine(new Vector2(0, 0), new Vector2(0, Size.Y), _lightColor, 1); // left light
            DrawLine(new Vector2(1, 0), new Vector2(1, Size.Y), _darkColor, 1); // right dark
        }

        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                QueueRedraw();
            }
        }

        public float Width
        {
            get => Size.X; set
            {
                Size = new Vector2(value, Size.Y);
                CustomMinimumSize = new Vector2(value, Size.Y);
            }
        }
        public float Height
        {
            get => Size.Y; set
            {
                Size = new Vector2(Size.X, value);
                CustomMinimumSize = new Vector2(2, value);
            }
        }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

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

        public object FrameworkNode => this;

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
