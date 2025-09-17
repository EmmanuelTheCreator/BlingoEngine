using Godot;
using static Godot.Input;
using AbstUI.Primitives;
using AbstUI.Inputs;
using AbstUI.FrameworkCommunication;


namespace AbstUI.LGodot.Inputs
{
    public interface IAbstGodotMouseHandler : IAbstFrameworkMouse
    {
        void HandleMouseMoveEvent(InputEventMouseMotion mouseMotionEvent, bool isInsideRect, float x, float y);
        void HandleMouseButtonEvent(InputEventMouseButton mouseButtonEvent, bool isInsideRect, float x, float y);

    }
    public class AbstGodotMouse : IAbstFrameworkMouse, IAbstGodotMouseHandler, IFrameworkFor<AbstMouse>
    {
        private Lazy<IAbstMouseInternal> _blingoMouse;
        private DateTime _lastClickTime = DateTime.MinValue;
        private const double DOUBLE_CLICK_TIME_LIMIT = 0.25;  // 250 milliseconds for double-click detection
        private bool _wasPressed = false;
        public static AMouseCursor LastCursor { get; protected set; } = AMouseCursor.Arrow;

        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public AbstGodotMouse(Lazy<IAbstMouseInternal> blingoMouse)
        {
            _blingoMouse = blingoMouse;
        }

        public void SetOffset(int x, int y)
        {
            OffsetX = x;
            OffsetY = y;
        }
        public void ReplaceMouseObj(IAbstMouse blingoMouse)
        {
            _blingoMouse = new Lazy<IAbstMouseInternal>(() => (IAbstMouseInternal)blingoMouse);
        }
        public void Release()
        {

        }
        public void HandleMouseMoveEvent(InputEventMouseMotion mouseMotionEvent, bool isInsideRect, float x, float y)
        {
            //Console.WriteLine($"Mouse Move: {mouseMotionEvent.Position.X}, {mouseMotionEvent.Position.Y}");
            //var x = mouseMotionEvent.Position.X + _offset.X;
            //var y = mouseMotionEvent.Position.Y + _offset.Y;
            _blingoMouse.Value.MouseH = x;
            _blingoMouse.Value.MouseV = y;
            if (isInsideRect)
                _blingoMouse.Value.DoMouseMove();
        }
      

        public void HandleMouseButtonEvent(InputEventMouseButton mouseButtonEvent, bool isInsideRect, float x, float y)
        {
            _blingoMouse.Value.MouseH = x;
            _blingoMouse.Value.MouseV = y;
            if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp || mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
            {
                var delta = mouseButtonEvent.ButtonIndex == MouseButton.WheelUp ? 1f : -1f;
                _blingoMouse.Value.DoMouseWheel(delta);
                return;
            }
            //Console.WriteLine(Name + ":" + mouseButtonEvent.Position.X + "x" + mouseButtonEvent.Position.Y + ":" + isInsideRect);
            // Handle Mouse Down event
            if (mouseButtonEvent.Pressed)
            {
                // mouse down must be inside rect, mouse up may not
                if (isInsideRect && x > 0 && y > 0)
                {
                    _wasPressed = true;
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                    {
                        // Handle Left Button Down
                        _blingoMouse.Value.MouseDown = true;
                        _blingoMouse.Value.LeftMouseDown = true;
                        DetectDoubleClick();
                        _blingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Handle Right Button Down
                        _blingoMouse.Value.RightMouseDown = true;
                        _blingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        // Handle Middle Button Down
                        _blingoMouse.Value.MiddleMouseDown = true;
                        _blingoMouse.Value.DoMouseDown();
                    }
                }
            }
            // Handle Mouse Up event
            else if (_wasPressed)
            {
                _wasPressed = false;
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    _blingoMouse.Value.MouseDown = false;
                    _blingoMouse.Value.LeftMouseDown = false;
                    _blingoMouse.Value.DoMouseUp();
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                {
                    _blingoMouse.Value.RightMouseDown = false;
                    _blingoMouse.Value.DoMouseUp();
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                {
                    _blingoMouse.Value.MiddleMouseDown = false;
                    _blingoMouse.Value.DoMouseUp();
                }
            }
        }

        private void DetectDoubleClick()
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Check if double-click occurred within the time limit
            if (_lastClickTime != DateTime.MinValue && (currentTime - _lastClickTime).TotalSeconds <= DOUBLE_CLICK_TIME_LIMIT)
                _blingoMouse.Value.DoubleClick = true;
            else
                _blingoMouse.Value.DoubleClick = false;

            _lastClickTime = currentTime; // Update last click time
        }

        public void HideMouse(bool state)
        {
            LastCursor = state? AMouseCursor.Hidden: AMouseCursor.Arrow;
            MouseMode = state ? MouseModeEnum.Hidden : MouseModeEnum.Visible;
        }
        public AMouseCursor GetCursor() => MouseMode== MouseModeEnum.Hidden? AMouseCursor.Hidden: LastCursor;
        public void SetCursor(AMouseCursor cursor)
        {
            if (LastCursor == cursor) return;
            LastCursor = cursor;
            if (cursor == AMouseCursor.Blank || cursor == AMouseCursor.Hidden)
            {
                MouseMode = MouseModeEnum.Hidden;
                return;
            }
            Console.WriteLine($"Swap cursor: {cursor}");
            MouseMode = MouseModeEnum.Visible;
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
