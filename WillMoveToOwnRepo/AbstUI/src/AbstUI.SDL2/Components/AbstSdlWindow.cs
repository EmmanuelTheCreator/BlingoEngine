using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;

namespace AbstUI.SDL2.Components;

internal class AbstSdlWindow : AbstSdlPanel, IAbstFrameworkWindow, IDisposable
{
    private readonly AbstSdlComponentFactory _factory;
    private IAbstWindowInternal _abstWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;



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

    public bool IsOpen => Visibility;

    public bool IsActiveWindow => _abstWindow.IsActivated;

    public IAbstMouse Mouse => throw new NotImplementedException();

    public IAbstKey AbstKey => throw new NotImplementedException();

    public AbstSdlWindow(AbstSdlComponentFactory factory) : base(factory)
    {
        _factory = factory;
        //var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        //var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        //_abstWindow.Init(this, mouse, key);
        Visibility = false;
    }
    
    public void Init(IAbstWindow instance)
    {
        _abstWindow = (IAbstWindowInternal)instance;
        _abstWindow.Init(this);
    }

    // TODO :  Resize SDL window.
    public void OnResize(int width, int height)
    {
        _abstWindow.ResizeFromFW(false,width, height);
    }

    public void Popup()
    {
        _factory.RootContext.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _abstWindow.RaiseWindowStateChanged(true);
    }

    public void PopupCentered()
    {
        APoint size = _factory.RootContext.GetWindowSize();

        X = (size.X - Width) / 2f;
        Y = (size.Y - Height) / 2f;
        Popup();
        _abstWindow.RaiseWindowStateChanged(true);
    }

    public void Hide()
    {
        Visibility = false;
        _factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _abstWindow.RaiseWindowStateChanged(false);
    }

    public void OpenWindow()
    {
        
    }

    public void CloseWindow()
    {
        
    }

    public void MoveWindow(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        SetSize(width, height);
    }

    public APoint GetPosition() => new APoint(X, Y);


    public APoint GetSize() => new APoint(Width, Height);

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

   
}
