using Godot;
using static Godot.Input;
using AbstUI.Primitives;
using AbstUI.Inputs;


namespace AbstUI.LGodot.Inputs
{
    public interface IAbstGodotMouseHandler: IAbstFrameworkMouse
    {
        void HandleMouseMoveEvent(InputEventMouseMotion mouseMotionEvent, bool isInsideRect, float x, float y);
        void HandleMouseButtonEvent(InputEventMouseButton mouseButtonEvent, bool isInsideRect, float x, float y);
        
    }
    public class AbstGodotMouse : IAbstFrameworkMouse, IAbstGodotMouseHandler
    {
        private Lazy<IAbstMouseInternal> _lingoMouse;
        private DateTime _lastClickTime = DateTime.MinValue;
        private const double DOUBLE_CLICK_TIME_LIMIT = 0.25;  // 250 milliseconds for double-click detection
        public AbstGodotMouse(Lazy<IAbstMouseInternal> lingoMouse)
        {
            _lingoMouse = lingoMouse;
        }
        public void ReplaceMouseObj(IAbstMouse lingoMouse)
        {
            _lingoMouse = new Lazy<IAbstMouseInternal>(() => (IAbstMouseInternal)lingoMouse);
        }
        public void Release()
        {

        }
        public void HandleMouseMoveEvent(InputEventMouseMotion mouseMotionEvent, bool isInsideRect, float x, float y)
        {
            //Console.WriteLine($"Mouse Move: {mouseMotionEvent.Position.X}, {mouseMotionEvent.Position.Y}");
            //var x = mouseMotionEvent.Position.X + _offset.X;
            //var y = mouseMotionEvent.Position.Y + _offset.Y;
            _lingoMouse.Value.MouseH = x;
            _lingoMouse.Value.MouseV = y;
            if (isInsideRect)
                _lingoMouse.Value.DoMouseMove();
        }
        private bool wasPressed = false;
        public void HandleMouseButtonEvent(InputEventMouseButton mouseButtonEvent, bool isInsideRect, float x, float y)
        {
            _lingoMouse.Value.MouseH = x;
            _lingoMouse.Value.MouseV = y;
            if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp || mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
            {
                var delta = mouseButtonEvent.ButtonIndex == MouseButton.WheelUp ? 1f : -1f;
                _lingoMouse.Value.DoMouseWheel(delta);
                return;
            }
            //Console.WriteLine(Name + ":" + mouseButtonEvent.Position.X + "x" + mouseButtonEvent.Position.Y + ":" + isInsideRect);
            // Handle Mouse Down event
            if (mouseButtonEvent.Pressed)
            {
                // mouse down must be inside rect, mouse up may not
                if (isInsideRect && x > 0 && y > 0)
                {
                    wasPressed = true;
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                    {
                        // Handle Left Button Down
                        _lingoMouse.Value.MouseDown = true;
                        _lingoMouse.Value.LeftMouseDown = true;
                        DetectDoubleClick();
                        _lingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Handle Right Button Down
                        _lingoMouse.Value.RightMouseDown = true;
                        _lingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        // Handle Middle Button Down
                        _lingoMouse.Value.MiddleMouseDown = true;
                        _lingoMouse.Value.DoMouseDown();
                    }
                }
            }
            // Handle Mouse Up event
            else if (wasPressed)
            {
                wasPressed = false;
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    _lingoMouse.Value.MouseDown = false;
                    _lingoMouse.Value.LeftMouseDown = false;
                    _lingoMouse.Value.DoMouseUp();
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                {
                    _lingoMouse.Value.RightMouseDown = false;
                    _lingoMouse.Value.DoMouseUp();
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                {
                    _lingoMouse.Value.MiddleMouseDown = false;
                    _lingoMouse.Value.DoMouseUp();
                }
            }
        }

        private void DetectDoubleClick()
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Check if double-click occurred within the time limit
            if (_lastClickTime != DateTime.MinValue && (currentTime - _lastClickTime).TotalSeconds <= DOUBLE_CLICK_TIME_LIMIT)
                _lingoMouse.Value.DoubleClick = true;
            else
                _lingoMouse.Value.DoubleClick = false;

            _lastClickTime = currentTime; // Update last click time
        }

        public void HideMouse(bool state)
        {
            MouseMode = state ? MouseModeEnum.Hidden : MouseModeEnum.Visible;
        }

        public void SetCursor(AMouseCursor cursor)
        {
            if (cursor == AMouseCursor.Blank)
            {
                MouseMode = MouseModeEnum.Hidden;
                return;
            }
            var godotCursor = ToGodotCursor(cursor);
            DisplayServer.Singleton.CursorSetShape(godotCursor);

        }
        /// <summary>
        /// Converts a AbstMouseCursor to the equivalent Godot CursorShape.
        /// </summary>
        public static DisplayServer.CursorShape ToGodotCursor(AMouseCursor cursor)
        {
            return cursor switch
            {
                AMouseCursor.Arrow => DisplayServer.CursorShape.Arrow,
                AMouseCursor.Cross => DisplayServer.CursorShape.Cross,
                AMouseCursor.Watch => DisplayServer.CursorShape.Wait,
                AMouseCursor.IBeam => DisplayServer.CursorShape.Ibeam,
                AMouseCursor.SizeAll => DisplayServer.CursorShape.Move,
                AMouseCursor.SizeNWSE => DisplayServer.CursorShape.Bdiagsize,
                AMouseCursor.SizeNESW => DisplayServer.CursorShape.Fdiagsize,
                AMouseCursor.SizeWE => DisplayServer.CursorShape.Hsize,
                AMouseCursor.SizeNS => DisplayServer.CursorShape.Vsize,
                AMouseCursor.UpArrow => DisplayServer.CursorShape.Arrow, // not correct
                AMouseCursor.Blank => DisplayServer.CursorShape.Arrow, // not correct
                AMouseCursor.Finger => DisplayServer.CursorShape.PointingHand,
                AMouseCursor.Drag => DisplayServer.CursorShape.Drag,
                AMouseCursor.Help => DisplayServer.CursorShape.Help,
                AMouseCursor.Wait => DisplayServer.CursorShape.Busy,
                AMouseCursor.NotAllowed => DisplayServer.CursorShape.Forbidden,
                _ => DisplayServer.CursorShape.Arrow
            };
        }


    }

}