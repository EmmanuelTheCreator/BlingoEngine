using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Inputs;
public class SdlMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<AbstMouse>
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _blingoMouse;
    private bool _hidden;
    protected nint _sdlCursor = nint.Zero;
    private AMouseCursor _lastCursor = AMouseCursor.Arrow;

    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    public SdlMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _blingoMouse = mouse;
    }
    ~SdlMouse()
    {
        if (_sdlCursor != nint.Zero)
            SDL.SDL_FreeCursor(_sdlCursor);
    }
    public virtual void Release()
    {
        if (_sdlCursor != nint.Zero)
        {
            SDL.SDL_FreeCursor(_sdlCursor);
            _sdlCursor = nint.Zero;
        }
    }
    public void SetMouse(AbstMouse<TAbstUIMouseEvent> mouse) => _blingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
        SDL.SDL_ShowCursor(state ? 0 : 1);
    }

    public void SetOffset(int x, int y)
    {
        OffsetX = x;
        OffsetY = y;
    }



    public void ReplaceMouseObj(IAbstMouse blingoMouse)
    {
        _blingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)blingoMouse);
    }

    public void ProcessEvent(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                _blingoMouse.Value.MouseH = e.motion.x;
                _blingoMouse.Value.MouseV = e.motion.y;
                _blingoMouse.Value.DoMouseMove();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                _blingoMouse.Value.MouseH = e.button.x;
                _blingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _blingoMouse.Value.MouseDown = true;
                    _blingoMouse.Value.LeftMouseDown = true;
                    _blingoMouse.Value.DoubleClick = e.button.clicks > 1;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _blingoMouse.Value.RightMouseDown = true;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _blingoMouse.Value.MiddleMouseDown = true;
                }
                _blingoMouse.Value.DoMouseDown();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                _blingoMouse.Value.MouseH = e.button.x;
                _blingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _blingoMouse.Value.MouseDown = false;
                    _blingoMouse.Value.LeftMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _blingoMouse.Value.RightMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _blingoMouse.Value.MiddleMouseDown = false;
                }
                _blingoMouse.Value.DoMouseUp();
                break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                SDL.SDL_GetMouseState(out var x, out var y);
                _blingoMouse.Value.MouseH = x;
                _blingoMouse.Value.MouseV = y;
                _blingoMouse.Value.DoMouseWheel(e.wheel.y);
                break;
        }
    }
    public virtual void SetCursor(AMouseCursor value)
    {
        _lastCursor = value;
        if (value == AMouseCursor.Hidden)
        {
            HideMouse(true);
            return;
        }
        else if (_hidden)
        {
            HideMouse(false);
        }
        SDL.SDL_SystemCursor sysCursor = value switch
        {
            AMouseCursor.Cross => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            AMouseCursor.Watch => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            AMouseCursor.IBeam => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
            AMouseCursor.SizeAll => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
            AMouseCursor.SizeNWSE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE,
            AMouseCursor.SizeNESW => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW,
            AMouseCursor.SizeWE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
            AMouseCursor.SizeNS => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
            AMouseCursor.UpArrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Blank => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Finger => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
            AMouseCursor.Drag => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
            AMouseCursor.Help => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Wait => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            AMouseCursor.NotAllowed => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO,
            AMouseCursor.Hidden => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO,
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW
        };

        if (_sdlCursor != nint.Zero)
            SDL.SDL_FreeCursor(_sdlCursor);

        _sdlCursor = SDL.SDL_CreateSystemCursor(sysCursor);
        SDL.SDL_SetCursor(_sdlCursor);
    }

    public AMouseCursor GetCursor() => _hidden? AMouseCursor.Hidden : _lastCursor;
}

