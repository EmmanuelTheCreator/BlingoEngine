using LingoEngine.Events;

namespace LingoEngine.Inputs.Events
{
    /// <summary>
    /// Has Mouse Within Event interface.
    /// </summary>
    public interface IHasMouseWithinEvent
    {
        void MouseWithin(LingoMouseEvent mouse);
    }
    /// <summary>
    /// Has Mouse Leave Event interface.
    /// </summary>
    public interface IHasMouseLeaveEvent
    {
        void MouseLeave(LingoMouseEvent mouse);
    }
    /// <summary>
    /// Has Mouse Down Event interface.
    /// </summary>
    public interface IHasMouseDownEvent
    {
        void MouseDown(LingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Up Event interface.
    /// </summary>
    public interface IHasMouseUpEvent
    {
        void MouseUp(LingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Move Event interface.
    /// </summary>
    public interface IHasMouseMoveEvent
    {
        void MouseMove(LingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Wheel Event interface.
    /// </summary>
    public interface IHasMouseWheelEvent
    {
        void MouseWheel(LingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Enter Event interface.
    /// </summary>
    public interface IHasMouseEnterEvent
    {
        void MouseEnter(LingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Exit Event interface.
    /// </summary>
    public interface IHasMouseExitEvent
    {
        void MouseExit(LingoMouseEvent mouse);
    }
}
