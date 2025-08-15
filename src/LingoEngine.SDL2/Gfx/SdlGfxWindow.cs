using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace LingoEngine.SDL2.Gfx;

internal class SdlGfxWindow : SdlGfxPanel, IAbstUIFrameworkGfxWindow, IDisposable
{
    private readonly SdlGfxFactory _factory;
    private readonly AbstUIGfxWindow _lingoWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;

    public SdlGfxWindow(AbstUIGfxWindow window, SdlGfxFactory factory) : base(factory)
    {
        _lingoWindow = window;
        _factory = factory;
        var mouse = ((IAbstUIMouseInternal)factory.RootContext.LingoMouse).CreateNewInstance(window);
        var key = ((AbstUIKey)factory.RootContext.LingoKey).CreateNewInstance(window);
        _lingoWindow.Init(this, mouse , key);
        Visibility = false;
    }


    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public new float Width
    {
        get => base.Width;
        set
        {
            if (Math.Abs(base.Width - value) > float.Epsilon)
                base.Width = value;
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            if (Math.Abs(base.Height - value) > float.Epsilon)
                base.Height = value;
        }
    }

    public bool IsPopup
    {
        get => _isPopup;
        set => _isPopup = value;
    }

    public bool Borderless
    {
        get => _borderless;
        set => _borderless = value;
    }

    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }


    // TODO :  Resize SDL window.
    public void OnResize(int width, int height)
    {
        _lingoWindow.Resize(width, height);
    }

    public void Popup()
    {
        _factory.RootContext.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void PopupCentered()
    {
        APoint size = _factory.RootContext.GetWindowSize();
       
        X = (size.X - Width) / 2f;
        Y = (size.Y - Height) / 2f;
        Popup();
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void Hide()
    {
        Visibility = false;
        _factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _lingoWindow.RaiseWindowStateChanged(false);
    }
}
