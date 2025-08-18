using AbstUI.Inputs;

namespace AbstUI.ImGui.Inputs;

/// <summary>
/// Simplified mouse handler for the ImGui backend.
/// </summary>
public class AbstImGuiMouse<TAbstUIMouseEvent> : IAbstFrameworkMouse where TAbstUIMouseEvent : AbstMouseEvent
{
    private Lazy<AbstMouse<TAbstUIMouseEvent>> _lingoMouse;

    public AbstImGuiMouse(Lazy<AbstMouse<TAbstUIMouseEvent>> mouse)
    {
        _lingoMouse = mouse;
    }

    public void HideMouse(bool state)
    {
        // ImGui handles cursor visibility internally.
    }

    public void Release()
    {
        // Nothing to release in the placeholder implementation.
    }

    public void ReplaceMouseObj(IAbstMouse lingoMouse) =>
        _lingoMouse = new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (AbstMouse<TAbstUIMouseEvent>)lingoMouse);
}

