using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Components;

internal class AbstBlazorWindow : AbstBlazorPanel, IDisposable, IAbstFrameworkWindow
{
    private readonly AbstBlazorComponentFactory _factory;
    private readonly IAbstWindowInternal _lingoWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;
    private IJSObjectReference? _module;
    private IAbstWindow _abstWindow;

    [Inject] private IJSRuntime JS { get; set; } = default!;

  

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
    public AColor BackgroundColor { get; set; }

    public bool IsActiveWindow => _lingoWindow.IsActivated;

    public bool IsOpen => Visibility;

    public IAbstMouse Mouse => _abstWindow.Mouse;

    public IAbstKey AbstKey => _abstWindow.Key;

    public AbstBlazorWindow(IAbstWindow window, AbstBlazorComponentFactory factory)
    {
        _lingoWindow = (IAbstWindowInternal)window;
        _factory = factory;
        //var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        //var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        //_lingoWindow.Init(this, mouse, key);
        Visibility = false;
    }


    public void Init(IAbstWindow instance)
    {
        _abstWindow = instance;
        instance.Init(this);
    }

    public void OnResize(int width, int height)
    {
        Width = width;
        Height = height;
        _lingoWindow.ResizeFromFW(false, width, height);
    }

    public void Popup()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.showBootstrapModal", Name);
        //_factory.RootContext.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void PopupCentered()
    {
        //APoint size = _factory.RootContext.GetWindowSize();

        //X = (size.X - Width) / 2f;
        //Y = (size.Y - Height) / 2f;
        Popup();
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    

    private void EnsureModule()
    {
        _module ??= JS.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js")
                     .AsTask().GetAwaiter().GetResult();
    }

    public void OpenWindow()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.showBootstrapModal", Name);
        //_factory.RootContext.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _lingoWindow.RaiseWindowStateChanged(true);
    }

    public void CloseWindow()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", Name);
        Visibility = false;
        //_factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _lingoWindow.RaiseWindowStateChanged(false);
    }

    public void MoveWindow(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        MoveWindow(x, y);
        Width = width;
        Height = height;
    }

    public APoint GetPosition() => new APoint(X, Y);

    public APoint GetSize() => new APoint(Width, Height);

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

  
}
