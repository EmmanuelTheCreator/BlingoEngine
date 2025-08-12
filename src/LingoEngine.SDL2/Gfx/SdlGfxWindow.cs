using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Core;
using LingoEngine.SDL2.SDLL;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;

namespace LingoEngine.SDL2.Gfx;

internal class SdlGfxWindow : SdlGfxPanel, ILingoFrameworkGfxWindow, IDisposable
{
    private readonly SdlFactory _factory;
    private readonly SdlMouse _mouseImpl;
    private readonly SdlKey _keyImpl;
    private event Action? _onOpen;
    private event Action? _onClose;
    private event Action<float, float>? _onResize;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;

    public SdlGfxWindow(LingoGfxWindow window, SdlFactory factory) : base(factory)
    {
        _factory = factory;

        LingoMouse? mouse = null;
        _mouseImpl = new SdlMouse(new Lazy<LingoMouse>(() => mouse!));
        _factory.RootContext.Mice.Add(_mouseImpl);
        mouse = new LingoMouse(_mouseImpl);

        _keyImpl = new SdlKey();
        _factory.RootContext.Keys.Add(_keyImpl);
        var key = new LingoKey(_keyImpl);
        _keyImpl.SetLingoKey(key);

        window.Init(this, mouse, key);
        Visibility = false;
    }

    public void Dispose()
    {
        _factory.RootContext.Mice.Remove(_mouseImpl);
        _factory.RootContext.Keys.Remove(_keyImpl);
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
            {
                base.Width = value;
                _onResize?.Invoke(base.Width, base.Height);
            }
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            if (Math.Abs(base.Height - value) > float.Epsilon)
            {
                base.Height = value;
                _onResize?.Invoke(base.Width, base.Height);
            }
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

    public new LingoColor BackgroundColor
    {
        get => base.BackgroundColor ?? LingoColorList.White;
        set => base.BackgroundColor = value;
    }

    event Action? ILingoFrameworkGfxWindow.OnOpen
    {
        add => _onOpen += value;
        remove => _onOpen -= value;
    }

    event Action? ILingoFrameworkGfxWindow.OnClose
    {
        add => _onClose += value;
        remove => _onClose -= value;
    }

    event Action<float, float>? ILingoFrameworkGfxWindow.OnResize
    {
        add => _onResize += value;
        remove => _onResize -= value;
    }

    public void Popup()
    {
        _factory.ComponentContainer.Activate(ComponentContext);
        Visibility = true;
        _onOpen?.Invoke();
    }

    public void PopupCentered()
    {
        SDL.SDL_GetWindowSize(_factory.RootContext.Window, out var w, out var h);
        X = (w - Width) / 2f;
        Y = (h - Height) / 2f;
        Popup();
    }

    public void Hide()
    {
        Visibility = false;
        _factory.ComponentContainer.Deactivate(ComponentContext);
        _onClose?.Invoke();
    }
}
