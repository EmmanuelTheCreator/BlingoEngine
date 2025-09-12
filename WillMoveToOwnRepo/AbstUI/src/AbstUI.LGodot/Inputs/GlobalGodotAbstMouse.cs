using AbstUI.LGodot;
using AbstUI.LGodot.Inputs;
using AbstUI.Primitives;
using Godot;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Inputs
{
    public class GlobalGodotAbstMouse : AbstMouse, IAbstGlobalMouse
    {
        public GlobalGodotAbstMouse(IAbstGodotRootNode rootNode)
            : base(CreateFrameworkMouse(rootNode, out var framework))
        {
            framework.ReplaceMouseObj(this);
        }

        private static IAbstFrameworkMouse CreateFrameworkMouse(IAbstGodotRootNode rootNode, out AbstGodotGlobalMouse framework)
        {
            framework = new AbstGodotGlobalMouse(rootNode);
            return framework;
        }
    }

    public partial class AbstGodotGlobalMouse : Node, IAbstFrameworkMouse, IAbstGodotMouseHandler, IFrameworkFor<GlobalGodotAbstMouse>
    {
        private readonly AbstGodotMouse _handler;
        private IAbstMouse? _mouse;

        public AbstGodotGlobalMouse(IAbstGodotRootNode rootNode)
        {
            _handler = new AbstGodotMouse(new Lazy<IAbstMouseInternal>(() => (IAbstMouseInternal)_mouse!));
            rootNode.RootNode.AddChild(this);
        }

        public override void _Input(InputEvent inputEvent)
        {
            var pos = GetViewport().GetMousePosition();
            if (inputEvent is InputEventMouseButton mouseButton)
            {
                _handler.HandleMouseButtonEvent(mouseButton, true, pos.X, pos.Y);
            }
            else if (inputEvent is InputEventMouseMotion mouseMotion)
            {
                _handler.HandleMouseMoveEvent(mouseMotion, true, pos.X, pos.Y);
            }
        }

        public void ReplaceMouseObj(IAbstMouse abstMouse)
        {
            _mouse = abstMouse;
            _handler.ReplaceMouseObj(abstMouse);
        }
        public void Release() => GetParent()?.RemoveChild(this);
        public void HideMouse(bool state) => _handler.HideMouse(state);
        public void SetCursor(AMouseCursor value) => _handler.SetCursor(value);
        public AMouseCursor GetCursor() => _handler.GetCursor();

        public void HandleMouseMoveEvent(InputEventMouseMotion mouseMotionEvent, bool isInsideRect, float x, float y)
            => _handler.HandleMouseMoveEvent(mouseMotionEvent, isInsideRect, x, y);

        public void HandleMouseButtonEvent(InputEventMouseButton mouseButtonEvent, bool isInsideRect, float x, float y)
            => _handler.HandleMouseButtonEvent(mouseButtonEvent, isInsideRect, x, y);
    }
}
