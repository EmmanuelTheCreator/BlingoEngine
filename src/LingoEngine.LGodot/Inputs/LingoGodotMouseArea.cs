using Godot;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Inputs;
using LingoEngine.LGodot.Inputs;


namespace LingoEngine.LGodot
{
    /// <summary>
    /// Communication between the Godot engine and the Lingo mouse object
    /// </summary>

    public partial class LingoGodotMouseArea : Area2D, IAbstFrameworkMouse, ILingoFrameworkMouse
    {
        private readonly LingoGodotMouse _handler;
        private CollisionShape2D _collisionShape2D = new();
        private RectangleShape2D _RectangleShape2D = new();
        private int _offsetX;
        private int _offsetY;

        public LingoGodotMouseArea(Node rootNode, Lazy<IAbstMouseInternal> lingoMouse)
        {
            Name = "MouseConnector";
            _handler = new LingoGodotMouse(lingoMouse);
            
            rootNode.AddChild(this);
            AddChild(_collisionShape2D);
            _RectangleShape2D.Size = new Vector2(1800, 1400);
            _collisionShape2D.Shape = _RectangleShape2D;
            _collisionShape2D.Name = "MouseDetectionCollisionShape";
        }
        public void ReplaceMouseObj(IAbstMouse lingoMouse) => _handler.ReplaceMouseObj(lingoMouse);
        public void Release()
        {
            RemoveChild(_collisionShape2D);
            GetParent().RemoveChild(this);
        }
        public void HideMouse(bool state)=> _handler.HideMouse(state);

        public void Resize(Vector2 vector2)
        {
            _RectangleShape2D.Size = vector2;
        }

        public void SetOffset(int x, int y)
        {
            _offsetX = x;
            _offsetY = y;
        }

        public void SetCursor(LingoMemberBitmap? image) => _handler.SetCursor(image);

        public void SetCursor(AMouseCursor value) => _handler.SetCursor(value);

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if (!Visible || !(@event is InputEventFromWindow)) return;
            //if (!_dragging && !_resizing && !GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            //    return;
            OnHandleTheEvent(@event);
        }

        //// This method will be called when Godot's input_event is triggered
        //public override void _Input(InputEvent inputEvent)
        //{
        ////    // Forward the input event to the _InputEvent method
        ////    _InputEvent(GetViewport(), inputEvent, 0);
        //}
        public override void _InputEvent(Viewport viewport, InputEvent inputEvent, int shapeIdx)
        { 
        }
        public void OnHandleTheEvent(InputEvent inputEvent)
        { 
            // Handle mouse button events (MouseDown and MouseUp)
            //var isInsideRect = _collisionShape2D.GetGlobalRect().HasPoint(GetGlobalMousePosition());
            var mousePos = GetLocalMousePosition();
            var rect = new Rect2(Position , _RectangleShape2D.Size);
            if (inputEvent is InputEventMouseButton mouseButtonEvent)
            {
                //Console.WriteLine("GodotMouseArea.Click:" + mouseButtonEvent.Position.X + "x" + mouseButtonEvent.Position.Y + " inside: " + rect.HasPoint(mouseButtonEvent.Position));
                var isInsideRect = rect.HasPoint(mouseButtonEvent.Position);
                _handler.HandleMouseButtonEvent(mouseButtonEvent, isInsideRect, mousePos.X- _offsetX, mousePos.Y- _offsetY);
            }
            // Handle Mouse Motion (MouseMove)
            else if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            {
                var isInsideRect = rect.HasPoint(mouseMotionEvent.Position);
                //Console.WriteLine("GodotMouseArea.MouseMove:" + mouseMotionEvent.Position.X + "x" + mouseMotionEvent.Position.Y+":"+ isInsideRect);
                if (isInsideRect)
                {
                    _handler.HandleMouseMoveEvent(mouseMotionEvent, isInsideRect, mousePos.X- _offsetX, mousePos.Y- _offsetY);
                }
            }
        }

       
    }

}