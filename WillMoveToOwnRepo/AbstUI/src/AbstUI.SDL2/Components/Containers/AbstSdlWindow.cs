using System;
using System.Runtime.InteropServices;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.Components;

namespace AbstUI.SDL2.Components.Containers;

public class AbstSdlWindow : AbstSdlPanel, IAbstFrameworkWindow, IHandleSdlEvent, IDisposable
{
    private readonly AbstSdlComponentFactory _factory;
    private IAbstWindowInternal _abstWindow;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;

    private ISdlFontLoadedByUser? _font;
    private SDL.SDL_Rect _closeRect;
    private bool _dragging;
    private int _dragOffsetX;
    private int _dragOffsetY;
    private const int TitleBarHeight = 24;



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

    public string WindowCode => _abstWindow.WindowCode;

    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }

    public bool IsOpen => Visibility;

    public bool IsActiveWindow => _abstWindow.IsActivated;

    public IAbstMouse Mouse => _abstWindow.Mouse;

    public IAbstKey AbstKey => _abstWindow.Key;

    private IAbstFrameworkNode? _content;
    public IAbstFrameworkNode? Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            RemoveAll();
            _content = value;
            if (value is IAbstFrameworkLayoutNode layout)
                AddItem(layout);
            _abstWindow.SetContentFromFW(value);
        }
    }

    public AbstSdlWindow(AbstSdlComponentFactory factory) : base(factory)
    {
        _factory = factory;
        ClipChildren = true;
        //var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        //var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        //_abstWindow.Init(this, mouse, key);
        Visibility = false;
    }

    public void Init(IAbstWindow instance)
    {
        _abstWindow = (IAbstWindowInternal)instance;
        _abstWindow.Init(this);
        instance.WindowTitleHeight = TitleBarHeight;
        _factory.WindowManager.Register(this);
    }

    // TODO :  Resize SDL window.
    public void OnResize(int width, int height)
    {
        _abstWindow.ResizeFromFW(false, width, height);
    }

    public void Popup()
    {
        Visibility = true;
        _factory.WindowManager.SetActiveWindow(this);
        _abstWindow.SetPositionFromFW((int)X, (int)Y);
        _abstWindow.ResizeFromFW(false, (int)Width, (int)Height);
        _abstWindow.RaiseWindowStateChanged(true);
    }

    public void PopupCentered()
    {
        APoint size = _factory.RootContext.GetWindowSize();

        X = (size.X - Width) / 2f;
        Y = (size.Y - Height) / 2f;
        _abstWindow.SetPositionFromFW((int)X, (int)Y);
        Popup();
    }

    public void Hide()
    {
        Visibility = false;
        _factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _abstWindow.RaiseWindowStateChanged(false);
    }

    internal void BringToFront()
        => _factory.RootContext.ComponentContainer.Activate(ComponentContext);

    public void OpenWindow() => Popup();

    public void CloseWindow() => Hide();

    public void MoveWindow(int x, int y)
    {
        X = x;
        Y = y;
        _abstWindow.SetPositionFromFW(x, y);
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        _abstWindow.SetPositionFromFW(x, y);
        SetSize(width, height);
    }

    public APoint GetPosition() => new APoint(X, Y);


    public APoint GetSize() => new APoint(Width, Height);

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        _abstWindow.ResizeFromFW(false, width, height);
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility)
            return default;

        var tex = (nint)base.Render(context);
        int w = (int)Width;

        if (_font == null)
            _font = context.SdlFontManager.GetTyped(this, null, 14);

        var prev = SDL.SDL_GetRenderTarget(context.Renderer);
        SDL.SDL_SetRenderTarget(context.Renderer, tex);

        SDL.SDL_SetRenderDrawColor(context.Renderer, 200, 200, 200, 255);
        var bar = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = TitleBarHeight };
        SDL.SDL_RenderFillRect(context.Renderer, ref bar);

        if (!string.IsNullOrEmpty(_title))
        {
            SDL.SDL_Color col = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
            nint surf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, _title, col);
            if (surf != nint.Zero)
            {
                var s = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
                nint t = SDL.SDL_CreateTextureFromSurface(context.Renderer, surf);
                SDL.SDL_FreeSurface(surf);
                var dst = new SDL.SDL_Rect
                {
                    x = 4,
                    y = (TitleBarHeight - s.h) / 2,
                    w = s.w,
                    h = s.h
                };
                SDL.SDL_RenderCopy(context.Renderer, t, nint.Zero, ref dst);
                SDL.SDL_DestroyTexture(t);
            }
        }

        int btnSize = TitleBarHeight - 4;
        _closeRect = new SDL.SDL_Rect { x = w - btnSize - 2, y = 2, w = btnSize, h = btnSize };
        SDL.SDL_SetRenderDrawColor(context.Renderer, 180, 0, 0, 255);
        SDL.SDL_RenderFillRect(context.Renderer, ref _closeRect);
        SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
        SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + 3, _closeRect.y + 3,
            _closeRect.x + _closeRect.w - 3, _closeRect.y + _closeRect.h - 3);
        SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + _closeRect.w - 3, _closeRect.y + 3,
            _closeRect.x + 3, _closeRect.y + _closeRect.h - 3);

        SDL.SDL_SetRenderTarget(context.Renderer, prev);
        return tex;
    }

    public void HandleEvent(AbstSDLEvent e)
    {
        if (!Visibility)
            return;

        switch (e.Event.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                int lx = e.Event.button.x - (int)X;
                int ly = e.Event.button.y - (int)Y;

                _factory.WindowManager.SetActiveWindow(this);

                if (lx >= _closeRect.x && lx <= _closeRect.x + _closeRect.w &&
                    ly >= _closeRect.y && ly <= _closeRect.y + _closeRect.h)
                {
                    Hide();
                    e.StopPropagation = true;
                    return;
                }

                if (ly <= TitleBarHeight)
                {
                    _dragging = true;
                    _dragOffsetX = lx;
                    _dragOffsetY = ly;
                    e.StopPropagation = true;
                }
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                _dragging = false;
                break;

            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                if (_dragging)
                {
                    X = e.Event.motion.x - _dragOffsetX;
                    Y = e.Event.motion.y - _dragOffsetY;
                    _abstWindow.SetPositionFromFW((int)X, (int)Y);
                    e.StopPropagation = true;
                }
                break;
        }
    }

    public override void Dispose()
    {
        _factory.WindowManager.Unregister(this);
        _font?.Release();
        base.Dispose();
    }
}
