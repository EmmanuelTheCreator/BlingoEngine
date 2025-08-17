using AbstUI.Inputs;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Inputs;

namespace LingoEngine.Blazor.Inputs;

public class LingoBlazorMouse : ILingoFrameworkMouse
{
    private Lazy<AbstMouse<LingoMouseEvent>> _lingoMouse;

    public LingoBlazorMouse(Lazy<AbstMouse<LingoMouseEvent>> mouse)
    {
        _lingoMouse = mouse;
    }

    public void HideMouse(bool state)
    {
        // Blazor integration pending
    }

    public void Release()
    {
        // cleanup resources if needed
    }

    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _lingoMouse = new Lazy<AbstMouse<LingoMouseEvent>>(() => (AbstMouse<LingoMouseEvent>)lingoMouse);
    }

    public void SetCursor(AMouseCursor cursorType)
    {
        // TODO: set system cursor when Blazor backend is available
    }

    public void SetCursor(LingoMemberBitmap? image)
    {
        // TODO: implement custom cursor images for Blazor
    }
}
