using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Inputs;
public class SdlMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _lingoMouse;
    private bool _hidden;
    protected nint _sdlCursor = nint.Zero;

    public SdlMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _lingoMouse = mouse;
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
    public void SetMouse(AbstMouse<TAbstUIMouseEvent> mouse) => _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
        SDL.SDL_ShowCursor(state ? 0 : 1);
    }





    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)lingoMouse);
    }

    public void ProcessEvent(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                _lingoMouse.Value.MouseH = e.motion.x;
                _lingoMouse.Value.MouseV = e.motion.y;
                _lingoMouse.Value.DoMouseMove();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = true;
                    _lingoMouse.Value.LeftMouseDown = true;
                    _lingoMouse.Value.DoubleClick = e.button.clicks > 1;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = true;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = true;
                }
                _lingoMouse.Value.DoMouseDown();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = false;
                    _lingoMouse.Value.LeftMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = false;
                }
                _lingoMouse.Value.DoMouseUp();
                break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                SDL.SDL_GetMouseState(out var x, out var y);
                _lingoMouse.Value.MouseH = x;
                _lingoMouse.Value.MouseV = y;
                _lingoMouse.Value.DoMouseWheel(e.wheel.y);
                break;
        }
    }
    public virtual void SetCursor(AMouseCursor value)
    {
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
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW
        };

        if (_sdlCursor != nint.Zero)
            SDL.SDL_FreeCursor(_sdlCursor);

        _sdlCursor = SDL.SDL_CreateSystemCursor(sysCursor);
        SDL.SDL_SetCursor(_sdlCursor);
    }



}
