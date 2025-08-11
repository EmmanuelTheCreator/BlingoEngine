
using LingoEngine.Events;

namespace LingoEngine.Inputs.Events
{
    public interface IHasMouseWithinEvent
    {
        void MouseWithin(LingoMouseEvent mouse);
    }
    public interface IHasMouseLeaveEvent
    {
        void MouseLeave(LingoMouseEvent mouse);
    }
    public interface IHasMouseDownEvent
    {
        void MouseDown(LingoMouseEvent mouse);
    }

    public interface IHasMouseUpEvent
    {
        void MouseUp(LingoMouseEvent mouse);
    }

    public interface IHasMouseMoveEvent
    {
        void MouseMove(LingoMouseEvent mouse);
    }


    public interface IHasMouseEnterEvent
    {
        void MouseEnter(LingoMouseEvent mouse);
    }

    public interface IHasMouseExitEvent
    {
        void MouseExit(LingoMouseEvent mouse);
    }
}
