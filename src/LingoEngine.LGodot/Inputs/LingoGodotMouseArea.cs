using Godot;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Inputs;


namespace LingoEngine.LGodot
{
    /// <summary>
    /// Communication between the Godot engine and the Lingo mouse object
    /// </summary>

    public partial class LingoGodotMouseArea : Area2D, IAbstUIFrameworkMouse
    {
        private readonly LingoGodotMouse _handler;
        private CollisionShape2D _collisionShape2D = new();
        private RectangleShape2D _RectangleShape2D = new();

        public LingoGodotMouseArea(Node rootNode, Lazy<LingoMouse> lingoMouse)
        {
            Name = "MouseConnector";
            _handler = new LingoGodotMouse(lingoMouse);
            
            rootNode.AddChild(this);
            AddChild(_collisionShape2D);
            _RectangleShape2D.Size = new Vector2(1800, 1400);
            _collisionShape2D.Shape = _RectangleShape2D;
            _collisionShape2D.Name = "MouseDetectionCollisionShape";
        }
        public void ReplaceMouseObj(IAbstUIMouse lingoMouse) => _handler.ReplaceMouseObj(lingoMouse);
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

        public void SetCursor(LingoMemberBitmap image) => _handler.SetCursor(image);

        public void SetCursor(AMouseCursor value) => _handler.SetCursor(value);

        // This method will be called when Godot's input_event is triggered
        public override void _InputEvent(Viewport viewport, InputEvent inputEvent, int shapeIdx)
        {
            // Handle mouse button events (MouseDown and MouseUp)
            //var isInsideRect = _collisionShape2D.GetGlobalRect().HasPoint(GetGlobalMousePosition());
            var mousePos = GetLocalMousePosition();
            var rect = new Rect2(Position , _RectangleShape2D.Size);
            if (inputEvent is InputEventMouseButton mouseButtonEvent)
            {
                Console.WriteLine("Click:" + mouseButtonEvent.Position.X + "x" + mouseButtonEvent.Position.Y + " inside: " + rect.HasPoint(mouseButtonEvent.Position));
                var isInsideRect = rect.HasPoint(mouseButtonEvent.Position);
                _handler.HandleMouseButtonEvent(mouseButtonEvent, isInsideRect, mousePos.X, mousePos.Y);
            }
            // Handle Mouse Motion (MouseMove)
            else if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            {
                var isInsideRect = rect.HasPoint(mouseMotionEvent.Position);
                _handler.HandleMouseMoveEvent(mouseMotionEvent, isInsideRect,mousePos.X,mousePos.Y);
            }
        }

       
    }

}