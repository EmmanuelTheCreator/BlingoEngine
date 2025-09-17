using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.FrameworkCommunication;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace AbstUI.Blazor.Inputs
{

    public class GlobalBlazorMouse : AbstMouse, IAbstGlobalMouse
    {
        public GlobalBlazorMouse(IJSRuntime js)
            : base(CreateFrameworkMouse(js, out var framework))
        {
            framework.ReplaceMouseObj(this);
        }

        private static IAbstFrameworkMouse CreateFrameworkMouse(IJSRuntime js, out AbstBlazorGlobalMouse<GlobalBlazorMouse, AbstMouseEvent> framework)
        {
            framework = new AbstBlazorGlobalMouse<GlobalBlazorMouse, AbstMouseEvent>(js);
            return framework;
        }
    }

    public class AbstBlazorGlobalMouse<TAbstMouseType, TAbstUIMouseEvent> : IAbstFrameworkMouse, IFrameworkFor<AbstMouse<TAbstUIMouseEvent>>
        where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        private readonly BlazorMouse<TAbstUIMouseEvent> _handler;
        private AbstMouse<TAbstUIMouseEvent>? _mouse;

        public AbstBlazorGlobalMouse(IJSRuntime js)
        {
            _handler = new BlazorMouse<TAbstUIMouseEvent>(new Lazy<AbstMouse<TAbstUIMouseEvent>>(() => (TAbstMouseType)_mouse!), js);
        }

        public void ReplaceMouseObj(IAbstMouse blingoMouse)
        {
            _mouse = (AbstMouse<TAbstUIMouseEvent>)blingoMouse;
            _handler.ReplaceMouseObj(blingoMouse);
        }

        public void Release()
        {
        }

        public void HideMouse(bool state) => _handler.HideMouse(state);

        public void SetCursor(AMouseCursor cursor)
        {
        }

        public void MouseMove(MouseEventArgs e) => _handler.MouseMove(e);

        public void MouseDown(MouseEventArgs e) => _handler.MouseDown(e);

        public void MouseUp(MouseEventArgs e) => _handler.MouseUp(e);

        public void Wheel(WheelEventArgs e) => _handler.Wheel(e);

        public AMouseCursor GetCursor()
        {
            return AMouseCursor.Arrow;
        }
    }
}

