using Godot;
using LingoEngine.Director.Core.Inputs;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot;
using LingoEngine.LGodot.Inputs;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace LingoEngine.Director.LGodot.Inputs
{
    public class GlobalLingoMouse : LingoMouse, IGlobalLingoMouse
    {
        public GlobalLingoMouse(LingoGodotRootNode rootNode)
            : base(CreateFrameworkMouse(rootNode, out var framework))
        {
            framework.ReplaceMouseObj(this);
        }

        private static ILingoFrameworkMouse CreateFrameworkMouse(LingoGodotRootNode rootNode, out LingoGodotGlobalMouse framework)
        {
            framework = new LingoGodotGlobalMouse(rootNode);
            return framework;
        }
    }

    public partial class LingoGodotGlobalMouse : Node, IAbstFrameworkMouse, ILingoFrameworkMouse
    {
        private readonly LingoGodotMouse _handler;
        private LingoMouse? _mouse;

        public LingoGodotGlobalMouse(LingoGodotRootNode rootNode)
        {
            _handler = new LingoGodotMouse(new Lazy<LingoMouse>(() => _mouse!));
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

        public void ReplaceMouseObj(IAbstMouse lingoMouse)
        {
            _mouse = (LingoMouse)lingoMouse;
            _handler.ReplaceMouseObj(lingoMouse);
        }
        public void Release() => GetParent()?.RemoveChild(this);
        public void HideMouse(bool state) => _handler.HideMouse(state);
        public void SetCursor(LingoMemberBitmap? image) => _handler.SetCursor(image);
        public void SetCursor(AMouseCursor value) => _handler.SetCursor(value);
    }
}
