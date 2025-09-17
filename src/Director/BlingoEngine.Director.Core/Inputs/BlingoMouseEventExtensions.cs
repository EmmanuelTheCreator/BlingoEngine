using AbstUI.Inputs;
using BlingoEngine.Events;
using BlingoEngine.Inputs;

namespace BlingoEngine.Director.Core.Inputs
{
    public static class BlingoMouseEventExtensions
    {
        public static BlingoMouseEvent Translate(this BlingoMouseEvent mouseEvent, BlingoMouse targetMouse)
        {
            var offset = ((IAbstMouseInternal)targetMouse).GetMouseOffset();
            var src = mouseEvent.Mouse;

            var prevH = targetMouse.MouseH;
            var prevV = targetMouse.MouseV;
            var prevDown = targetMouse.MouseDown;
            var prevUp = targetMouse.MouseUp;
            var prevLeftDown = targetMouse.LeftMouseDown;
            var prevRightDown = targetMouse.RightMouseDown;
            var prevRightUp = targetMouse.RightMouseUp;
            var prevMiddleDown = targetMouse.MiddleMouseDown;
            var prevDouble = targetMouse.DoubleClick;
            var prevWheel = targetMouse.WheelDelta;

            targetMouse.MouseH = mouseEvent.MouseH - offset.Left;
            targetMouse.MouseV = mouseEvent.MouseV - offset.Top;
            targetMouse.MouseDown = src.MouseDown;
            targetMouse.MouseUp = src.MouseUp;
            targetMouse.LeftMouseDown = src.LeftMouseDown;
            targetMouse.RightMouseDown = src.RightMouseDown;
            targetMouse.RightMouseUp = src.RightMouseUp;
            targetMouse.MiddleMouseDown = src.MiddleMouseDown;
            targetMouse.DoubleClick = src.DoubleClick;
            targetMouse.WheelDelta = src.WheelDelta;

            var translated = new BlingoMouseEvent(targetMouse, mouseEvent.Type);

            targetMouse.MouseH = prevH;
            targetMouse.MouseV = prevV;
            targetMouse.MouseDown = prevDown;
            targetMouse.MouseUp = prevUp;
            targetMouse.LeftMouseDown = prevLeftDown;
            targetMouse.RightMouseDown = prevRightDown;
            targetMouse.RightMouseUp = prevRightUp;
            targetMouse.MiddleMouseDown = prevMiddleDown;
            targetMouse.DoubleClick = prevDouble;
            targetMouse.WheelDelta = prevWheel;

            return translated;
        }
    }
}

