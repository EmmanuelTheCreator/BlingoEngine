using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Inputs;
public class BlazorMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse
        where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _lingoMouse;
    private bool _hidden;
    protected nint _sdlCursor = nint.Zero;

    public BlazorMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _lingoMouse = mouse;
    }
    ~BlazorMouse()
    {
        if (_sdlCursor != nint.Zero)
            Blazor.Blazor_FreeCursor(_sdlCursor);
    }
    public virtual void Release()
    {
        if (_sdlCursor != nint.Zero)
        {
            Blazor.Blazor_FreeCursor(_sdlCursor);
            _sdlCursor = nint.Zero;
        }
    }
    public void SetMouse(AbstMouse<TAbstUIMouseEvent> mouse) => _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
        Blazor.Blazor_ShowCursor(state ? 0 : 1);
    }





    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)lingoMouse);
    }

    public void ProcessEvent(Blazor.Blazor_Event e)
    {
        switch (e.type)
        {
            case Blazor.Blazor_EventType.Blazor_MOUSEMOTION:
                _lingoMouse.Value.MouseH = e.motion.x;
                _lingoMouse.Value.MouseV = e.motion.y;
                _lingoMouse.Value.DoMouseMove();
                break;
            case Blazor.Blazor_EventType.Blazor_MOUSEBUTTONDOWN:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == Blazor.Blazor_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = true;
                    _lingoMouse.Value.LeftMouseDown = true;
                    _lingoMouse.Value.DoubleClick = e.button.clicks > 1;
                }
                else if (e.button.button == Blazor.Blazor_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = true;
                }
                else if (e.button.button == Blazor.Blazor_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = true;
                }
                _lingoMouse.Value.DoMouseDown();
                break;
            case Blazor.Blazor_EventType.Blazor_MOUSEBUTTONUP:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == Blazor.Blazor_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = false;
                    _lingoMouse.Value.LeftMouseDown = false;
                }
                else if (e.button.button == Blazor.Blazor_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = false;
                }
                else if (e.button.button == Blazor.Blazor_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = false;
                }
                _lingoMouse.Value.DoMouseUp();
                break;
            case Blazor.Blazor_EventType.Blazor_MOUSEWHEEL:
                Blazor.Blazor_GetMouseState(out var x, out var y);
                _lingoMouse.Value.MouseH = x;
                _lingoMouse.Value.MouseV = y;
                _lingoMouse.Value.DoMouseWheel(e.wheel.y);
                break;
        }
    }
    public virtual void SetCursor(AMouseCursor value)
    {
        Blazor.Blazor_SystemCursor sysCursor = value switch
        {
            AMouseCursor.Cross => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_CROSSHAIR,
            AMouseCursor.Watch => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_WAIT,
            AMouseCursor.IBeam => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_IBEAM,
            AMouseCursor.SizeAll => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZEALL,
            AMouseCursor.SizeNWSE => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZENWSE,
            AMouseCursor.SizeNESW => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZENESW,
            AMouseCursor.SizeWE => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZEWE,
            AMouseCursor.SizeNS => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZENS,
            AMouseCursor.UpArrow => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Blank => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Finger => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_HAND,
            AMouseCursor.Drag => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_SIZEALL,
            AMouseCursor.Help => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_ARROW,
            AMouseCursor.Wait => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_WAIT,
            AMouseCursor.NotAllowed => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_NO,
            _ => Blazor.Blazor_SystemCursor.Blazor_SYSTEM_CURSOR_ARROW
        };

        if (_sdlCursor != nint.Zero)
            Blazor.Blazor_FreeCursor(_sdlCursor);

        _sdlCursor = Blazor.Blazor_CreateSystemCursor(sysCursor);
        Blazor.Blazor_SetCursor(_sdlCursor);
    }



}
