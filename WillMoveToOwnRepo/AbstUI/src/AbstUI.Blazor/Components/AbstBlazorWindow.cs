using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Components;

internal class AbstBlazorWindow : AbstBlazorPanel, IAbstFrameworkWindow, IDisposable
{
    private readonly AbstBlazorComponentFactory _factory;
    private readonly AbstWindow _lingoWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;
    private IJSObjectReference? _module;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    public AbstBlazorWindow(AbstWindow window, AbstBlazorComponentFactory factory) 
    {
        _lingoWindow = window;
        _factory = factory;
        //var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        //var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        //_lingoWindow.Init(this, mouse, key);
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


    public void OnResize(int width, int height)
    {
        Width = width;
        Height = height;
        _lingoWindow.Resize(width, height);
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

    public void Hide()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", Name);
        Visibility = false;
        //_factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _lingoWindow.RaiseWindowStateChanged(false);
    }

    private void EnsureModule()
    {
        _module ??= JS.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js")
                     .AsTask().GetAwaiter().GetResult();
    }
}
