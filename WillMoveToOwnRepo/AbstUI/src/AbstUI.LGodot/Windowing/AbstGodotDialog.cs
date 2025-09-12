using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using Godot;

namespace AbstUI.LGodot.Windowing
{
    public partial class AbstGodotDialog : AbstGodotDialogT<AbstDialog>, IFrameworkForInitializable<AbstDialog>, IAbstFrameworkDialog<AbstDialog>
    {
        
    }
    public partial class AbstGodotDialogT<T> : Window, IAbstFrameworkDialog<T>, IFrameworkFor<T>
        where T: AbstDialog
    {
        private IAbstDialog _lingoDialog = null!;
        private Action<AColor> _setBackgroundColorMethod = c => { };
        private AColor _backgroundColor;

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                _setBackgroundColorMethod(value);
            }
        }
        public bool IsOpen { get; set; }
        public bool IsPopup { get; set; }
        public bool IsActiveWindow => IsOpen;
        public float X { get => Position.X; set { Position = new Vector2I((int)value, Position.Y); } }
        public float Y { get => Position.Y; set { Position = new Vector2I(Position.X, (int)value); } }
        public bool Visibility { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public AMargin Margin { get; set; }
        public int ZIndex { get; set; }
        public object FrameworkNode => this;

        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public IAbstMouse Mouse => _lingoDialog.Mouse;

        public IAbstKey Key => _lingoDialog.Key;

        public event Action<bool>? OnWindowStateChanged;

        public AbstGodotDialogT()
        {
            Exclusive = true;
            PopupWindow = true;
            Unresizable = true;
            TransparentBg = true;
            Transparent = true;
            //AlwaysOnTop = true; // <- blocks combo boxes
            ReplaceIconColor(this, "close", new Color("#777777"));
            ReplaceIconColor(this, "close_hl", Colors.Black);
            ReplaceIconColor(this, "close_pressed", Colors.Black);
            CloseRequested += AbstGodotDialog_CloseRequested;
            AboutToPopup += AbstGodotDialog_AboutToPopup;
        }

        private void AbstGodotDialog_AboutToPopup()
        {
            OnWindowStateChanged?.Invoke(true);
        }

        private void AbstGodotDialog_CloseRequested()
        {
            OnWindowStateChanged?.Invoke(false);
        }

        public virtual void Init(AbstDialog lingoDialog)
        {
            _lingoDialog = lingoDialog;
            lingoDialog.Init(this);
        }
        protected override void Dispose(bool disposing)
        {
            CloseRequested -= AbstGodotDialog_CloseRequested;
            AboutToPopup -= AbstGodotDialog_AboutToPopup;
            base.Dispose(disposing);
        }

        public override void _Ready()
        {
            //RenderingServer.SetDefaultClearColor(new Color(0,0,0,0)); // RGB [0..1]
        }
        public void Popup() => base.Popup();

        public void PopupCentered() =>
            base.PopupCentered();


        public void AddItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode) => base.AddChild(abstFrameworkLayoutNode.FrameworkNode as Node);


        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() =>
            GetChildren().OfType<IAbstFrameworkLayoutNode>();
        public void RemoveItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode) =>
            base.RemoveChild(abstFrameworkLayoutNode.FrameworkNode as Node);

        public void SetPositionAndSize(int x, int y, int width, int height)
        {
            Position = new Vector2I(x, y);
            SetSize(width, height);
        }

        public void SetSize(int width, int height)
        {
            Size = new Vector2I(width, height);
        }

        APoint IAbstFrameworkDialog.GetPosition()
            => new APoint(Position.X, Position.Y);

        APoint IAbstFrameworkDialog.GetSize()
            => new APoint(Size.X, Size.Y);


        public static void ReplaceIconColor(Window dialog, string name, Color colorNew)
        {
            var closeButton = dialog.GetThemeIcon(name);
            ImageTexture tinted = CreateTintedIcon(closeButton, colorNew);
            dialog.AddThemeIconOverride(name, tinted);
        }
        public static ImageTexture CreateTintedIcon(Texture2D closeButton, Color colorNew)
        {
            var image = closeButton.GetImage();
            image.Convert(Image.Format.Rgba8);

            for (int y = 0; y < image.GetHeight(); y++)
            {
                for (int x = 0; x < image.GetWidth(); x++)
                {
                    var color = image.GetPixel(x, y);
                    color = new Color(colorNew.R, colorNew.G, colorNew.B, color.A); // tint red
                    image.SetPixel(x, y, color);
                }
            }

            var tinted = ImageTexture.CreateFromImage(image);

            return tinted;
        }

        internal void SetBackgroundColorMethod(Action<AColor> value) => _setBackgroundColorMethod = value;
    }
}
