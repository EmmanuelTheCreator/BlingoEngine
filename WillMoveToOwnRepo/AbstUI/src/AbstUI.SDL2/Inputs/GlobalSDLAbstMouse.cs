using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Inputs
{

    public class GlobalSDLAbstMouse : AbstMouse, IAbstGlobalMouse
    {
        public GlobalSDLAbstMouse()
            : base(CreateFrameworkMouse(out var framework))
        {
            framework.ReplaceMouseObj(this);
        }

        private static IAbstFrameworkMouse CreateFrameworkMouse(out AbstSdlGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent> framework)
        {
            framework = new AbstSdlGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent>();
            return framework;
        }
    }

    public class AbstSdlGlobalMouse<TAbstMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<AbstMouse<TAbstUIMouseEvent>>
        where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        private readonly SdlMouse<TAbstUIMouseEvent> _handler;
        private AbstMouse<TAbstUIMouseEvent>? _mouse;

        public AbstSdlGlobalMouse()
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
        }

        public void HideMouse(bool state) => _handler.HideMouse(state);

        public void SetCursor(AMouseCursor cursor) => _handler.SetCursor(cursor);
        public AMouseCursor GetCursor() => _handler.GetCursor();

        public void ProcessEvent(SDL.SDL_Event e)
        {
            _handler.ProcessEvent(e);
        }

    }
}
