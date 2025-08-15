using Godot;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Gfx;
using LingoEngine.Inputs;
using LingoEngine.LGodot.Core;
using LingoEngine.LGodot.Primitives;
using LingoEngine.LGodot.Styles;
using static Godot.Control;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxWindow"/>.
    /// </summary>
    public partial class LingoGodotWindow : Window, ILingoFrameworkGfxWindow, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private readonly List<ILingoFrameworkGfxLayoutNode> _nodes = new();
        private readonly Panel _panel;
        private readonly StyleBoxFlat _panelStyle;
        private readonly LingoGfxWindow _lingoWindow;
        private readonly LingoGodotRootNode _rootNode;
        protected readonly LingoGodotMouse _MouseFrameworkObj;
        private bool _isPopup;
        private int TitleBarHeight = 0;


        #region Properties

        public float X { get => Position.X; set => Position = new Vector2I((int)value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2I(Position.X, (int)value); }
        public float Width
        {
            get => Size.X;
            set
            {
                Size = new Vector2I((int)value, Size.Y);
                _panel.Size = Size;
            }
        }
        public float Height
        {
            get => Size.Y;
            set
            {
                Size = new Vector2I(Size.X, (int)value);
                _panel.Size = Size;
            }
        }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public new string Title { get => base.Title; set => base.Title = value; }
        public bool IsPopup
        {
            get => _isPopup;
            set
            {
                _isPopup = value;
                Unresizable = value;
                Exclusive = value;
            }
        }
        public AColor BackgroundColor
        {
            get => _panelStyle.BgColor.ToLingoColor();
            set => _panelStyle.BgColor = value.ToGodotColor();
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

        public object FrameworkNode => this;


        #endregion


        public LingoGodotWindow(LingoGfxWindow window, ILingoGodotStyleManager lingoGodotStyleManager, LingoGodotRootNode rootNode)
        {
            _lingoWindow = window;
            _rootNode = rootNode;
            LingoMouse? mouse = null;
            _MouseFrameworkObj = new LingoGodotMouse(new Lazy<LingoMouse>(() => (LingoMouse)mouse!));
            mouse = new LingoMouse(_MouseFrameworkObj);

            LingoKey? key = null;
            var impl = new LingoGodotKey(this, new Lazy<LingoKey>(() => key!));
            key = new LingoKey(impl);
            impl.SetLingoKey(key);

            _lingoWindow.Init(this,mouse,key);
            //Borderless = true;
            //ExtendToTitle = true;
            Theme = lingoGodotStyleManager.GetTheme(LingoGodotThemeElementType.PopupWindow);

            _panel = new Panel
            {
                Name = "WindowLayoutPanel",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };
            _panelStyle = new StyleBoxFlat
            {
                BgColor = AColors.White.ToGodotColor(),

            };
            _panel.AddThemeStyleboxOverride("panel", _panelStyle);
            AddChild(_panel);
            CloseRequested += Hide;

            ReplaceIconColor(this, "close", new Color("#777777"));
            ReplaceIconColor(this, "close_hl", Colors.Black);
            ReplaceIconColor(this, "close_pressed", Colors.Black);

            _rootNode.RootNode.GetTree().Root.AddChild(this);
            
        }


        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if (!Visible) return;
            OnHandleTheEvent(@event);
        }

        //public override void _GuiInput(InputEvent @event)
        //{
        //    base._GuiInput(@event);
        //    if (!useGuiInput || !Visible) return;
        //    OnHandleTheEvent(@event);
        //}
        protected virtual void OnHandleTheEvent(InputEvent @event)
        {
            var isInsideRect = _panel.GetGlobalRect().HasPoint(_panel.GetGlobalMousePosition());
            if (!isInsideRect) return;
            var mousePos = _panel.GetLocalMousePosition();
            //Console.WriteLine(Name + ":" + mousePos.X + "x" + mousePos.Y + ":" + isInsideRect);
            // Handle mouse button events (MouseDown and MouseUp)
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (!_lingoWindow.Visibility)
                    return;
                _MouseFrameworkObj.HandleMouseButtonEvent(mouseButtonEvent, isInsideRect, mousePos.X, mousePos.Y - TitleBarHeight);
                //Console.WriteLine(Name + ":" + mousePos.X + "x" + mousePos.Y+":"+ isInsideRect);
            }
            // Handle Mouse Motion (MouseMove)
            else if (@event is InputEventMouseMotion mouseMotionEvent)
                _MouseFrameworkObj.HandleMouseMoveEvent(mouseMotionEvent, isInsideRect, mousePos.X, mousePos.Y - TitleBarHeight);
        }

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                _panel.AddChild(node);
            _nodes.Add(child);
        }

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                _panel.RemoveChild(node);
            _nodes.Remove(child);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();

        public void Popup()
        {
            base.Popup();
            _lingoWindow.RaiseWindowStateChanged(true);
        }
        public void PopupCentered()
        {
            base.PopupCentered();
            _lingoWindow.RaiseWindowStateChanged(true);
        }
        public new void Hide()
        {
            base.Hide();
            _lingoWindow.RaiseWindowStateChanged(false);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == 1008)// NotificationResized
            {
                _panel.Size = Size;
                _lingoWindow.Resize(Size.X, Size.Y);
                
            }
        }
       
        private static void ReplaceIconColor(Window dialog, string name, Color colorNew)
        {
            var icon = dialog.GetThemeIcon(name);
            ImageTexture tinted = CreateTintedIcon(icon, colorNew);
            dialog.AddThemeIconOverride(name, tinted);
        }

        private static ImageTexture CreateTintedIcon(Texture2D icon, Color colorNew)
        {
            var image = icon.GetImage();
            image.Convert(Image.Format.Rgba8);
            for (int y = 0; y < image.GetHeight(); y++)
            {
                for (int x = 0; x < image.GetWidth(); x++)
                {
                    var color = image.GetPixel(x, y);
                    color = new Color(colorNew.R, colorNew.G, colorNew.B, color.A);
                    image.SetPixel(x, y, color);
                }
            }
            return ImageTexture.CreateFromImage(image);
        }

    }
}
