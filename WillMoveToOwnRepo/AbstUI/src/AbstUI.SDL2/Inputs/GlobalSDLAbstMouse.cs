using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Inputs
{
    
    public class GlobalSDLAbstMouse : AbstMouse, IAbstGlobalMouse
    {

        public GlobalSDLAbstMouse(IAbstSDLRootContext rootNode)
            : base(CreateFrameworkMouse(rootNode, out var framework))
        {
            framework.ReplaceMouseObj(this);
            rootNode.GlobalMouse = this;
        }

        private static IAbstFrameworkMouse CreateFrameworkMouse(IAbstSDLRootContext rootNode, out AbstGodotGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent> framework)
        {
            framework = new AbstGodotGlobalMouse<GlobalSDLAbstMouse,AbstMouseEvent>(rootNode);
            return framework;
        }
    }

    public partial class AbstGodotGlobalMouse<TAbstMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse
        where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        private readonly SdlMouse<TAbstUIMouseEvent> _handler;
        private AbstMouse<TAbstUIMouseEvent>? _mouse;

        public AbstGodotGlobalMouse(IAbstSDLRootContext rootNode)
        {
            
            _handler = new SdlMouse<TAbstUIMouseEvent>(new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (TAbstMouseType)_mouse!));
        }

       
        public void ReplaceMouseObj(IAbstMouse lingoMouse)
        {
            _mouse = (AbstMouse<TAbstUIMouseEvent>)lingoMouse;
            _handler.ReplaceMouseObj(lingoMouse);
        }
        public void Release()
        {
            //GetParent()?.RemoveChild(this);
        }

        public void HideMouse(bool state) => _handler.HideMouse(state);

        public void SetCursor(AMouseCursor cursor)
        {
            
        }

        public void ProcessEvent(SDL.SDL_Event e)
        {
            _handler.ProcessEvent(e);
        }
    }
}
