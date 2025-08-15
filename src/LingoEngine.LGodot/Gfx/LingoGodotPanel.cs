using Godot;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxPanel"/>.
    /// </summary>
    public partial class LingoGodotPanel : Panel, ILingoFrameworkGfxPanel, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private AColor? _background = null;
        private AColor? _borderColor = null;
        private float _borderWidth =0;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();

        public LingoGodotPanel(LingoGfxPanel panel)
        {
            MouseFilter = MouseFilterEnum.Ignore;
            panel.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
            //SizeFlagsHorizontal = SizeFlags.ExpandFill;
            //AddThemeStyleboxOverride("panel", _style);
            Set("theme_override_styles/panel", _style);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); } }// CustomMinimumSize = new Vector2(value, Size.Y); } } 
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                ApplyMargin();
            }
        }
        public object FrameworkNode => this;



        private readonly List<ILingoFrameworkGfxLayoutNode> _nodes = new List<ILingoFrameworkGfxLayoutNode>();
        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
            {
                base.AddChild(node);

                if (node is Control control)
                {
                    // Apply positioning
                    control.Position = new Vector2(child.X, child.Y);

                    // Important: disable anchors
                    control.AnchorLeft = 0;
                    control.AnchorTop = 0;
                    control.AnchorRight = 0;
                    control.AnchorBottom = 0;

                }
            }
            _nodes.Add(child);  
        }


        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                RemoveChild(node);
            _nodes.Remove(child);
        }
        public void RemoveAll()
        {
            foreach (Node child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }
            _nodes.Clear();
        }
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();
        //public override void _Draw()
        //{
        //    DrawRect(new Rect2(Vector2.Zero, Size), _background.ToGodotColor());
        //    if (_borderWidth > 0)
        //        DrawRect(new Rect2(Vector2.Zero, Size), _border.ToGodotColor(), false, _borderWidth);

        //    //foreach (var child in GetItems())
        //    //{
        //    //    if (child is Control c)
        //    //        DrawCircle(c.Position, 2, Colors.Red);
        //    //}
        //}
        public AColor? BackgroundColor
        {
            get => _background;
            set
            {
                _background = value;
                ApplyStyle();
            }
        }

        public AColor? BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                ApplyStyle();
            }
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                ApplyStyle();
            }
        }

        public new void Dispose()
        {
            RemoveAll();
            base.Dispose();
            QueueFree();
        }

        private void ApplyMargin()
        {
            AddThemeConstantOverride("margin_left", (int)_margin.Left);
            AddThemeConstantOverride("margin_right", (int)_margin.Right);
            AddThemeConstantOverride("margin_top", (int)_margin.Top);
            AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }

        private void ApplyStyle()
        {
            _style.BgColor = _background != null? _background.Value.ToGodotColor() : Colors.Transparent;
            _style.BorderColor = _borderColor != null ? _borderColor.Value.ToGodotColor() : Colors.Transparent;
            _style.BorderWidthTop = _style.BorderWidthBottom = _style.BorderWidthLeft = _style.BorderWidthRight = (int)_borderWidth;
        }
    }
    public partial class LingoGodotLayoutWrapper : MarginContainer, ILingoFrameworkGfxLayoutWrapper
    {
        private LingoGfxLayoutWrapper _lingoLayoutWrapper;
        public object FrameworkNode => this;

        public LingoGodotLayoutWrapper(LingoGfxLayoutWrapper layoutWrapper)
        {
            _lingoLayoutWrapper = layoutWrapper;
            layoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;

            if (content is Node node)
            {
                AddChild(node);
                if (content is Control control)
                {
                    control.SizeFlagsHorizontal = SizeFlags.Fill;
                    control.SizeFlagsVertical = SizeFlags.Fill;
                }
            }
            else
            {
                // todo: use ILogger
                //GD.PushError($"Content of layout wrapper '{layoutWrapper.Name}' is not a Control: {content.GetType().Name}");
            }
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => Size.X;
            set => Size = new Vector2(value, Size.Y);
        }
        public float Height
        {
            get => Size.Y;
            set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value);
        }

        public AMargin Margin
        {
            get => new AMargin(
                GetThemeConstant("margin_top"),
                GetThemeConstant("margin_right"),
                GetThemeConstant("margin_bottom"),
                GetThemeConstant("margin_left"));
            set
            {
                AddThemeConstantOverride("margin_top", (int)value.Top);
                AddThemeConstantOverride("margin_right", (int)value.Right);
                AddThemeConstantOverride("margin_bottom", (int)value.Bottom);
                AddThemeConstantOverride("margin_left", (int)value.Left);
            }
        }

        public new string Name { get => base.Name; set => base.Name = value; }
        public bool Visibility { get => Visible; set => Visible = value; }

        public new void Dispose()
        {
            base.Dispose();
            QueueFree();
        }
    }

}
