using BlingoEngine.Events;

namespace BlingoEngine.Inputs.Events
{
    /// <summary>
    /// Has Mouse Within Event interface.
    /// </summary>
    public interface IHasMouseWithinEvent
    {
        void MouseWithin(BlingoMouseEvent mouse);
    }
    /// <summary>
    /// Has Mouse Leave Event interface.
    /// </summary>
    public interface IHasMouseLeaveEvent
    {
        void MouseLeave(BlingoMouseEvent mouse);
    }
    /// <summary>
    /// Has Mouse Down Event interface.
    /// </summary>
    public interface IHasMouseDownEvent
    {
        void MouseDown(BlingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Up Event interface.
    /// </summary>
    public interface IHasMouseUpEvent
    {
        void MouseUp(BlingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Move Event interface.
    /// </summary>
    public interface IHasMouseMoveEvent
    {
        void MouseMove(BlingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Wheel Event interface.
    /// </summary>
    public interface IHasMouseWheelEvent
    {
        void MouseWheel(BlingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Enter Event interface.
    /// </summary>
    public interface IHasMouseEnterEvent
    {
        void MouseEnter(BlingoMouseEvent mouse);
    }

    /// <summary>
    /// Has Mouse Exit Event interface.
    /// </summary>
    public interface IHasMouseExitEvent
    {
        void MouseExit(BlingoMouseEvent mouse);
    }
}

