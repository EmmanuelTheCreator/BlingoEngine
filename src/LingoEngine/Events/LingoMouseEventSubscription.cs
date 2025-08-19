using AbstUI.Inputs;
using LingoEngine.Inputs;

namespace LingoEngine.Events
{
   
    public class LingoMouseEvent : AbstMouseEvent
    {
        public new LingoMouse Mouse { get; }

        public LingoMouseEvent(LingoMouse lingoMouse, AbstMouseEventType type)
            :base(lingoMouse, type)
        {
            Mouse = lingoMouse;
        }
        public LingoMouseEvent(AbstMouseEvent mouseEvent, LingoMouse mouse)
            :base(mouse, mouseEvent.Type)
        {
            Mouse = mouse;
            Type = mouseEvent.Type ;
            ContinuePropation = mouseEvent.ContinuePropation ;
        }
    }

    public interface ILingoMouseEventHandler : IAbstMouseEventHandler<LingoMouseEvent>
    {
    }
    public interface ILingoMouseEventSubscription
    {
        void Release();
    }

    //public class LingoMouseEventSubscription : IAbstUIMouseSubscription
    //{
    //    private readonly ILingoMouseEventHandler _target;
    //    private readonly Action<LingoMouseEventSubscription> _release;

    //    public LingoMouseEventSubscription(ILingoMouseEventHandler target, Action<LingoMouseEventSubscription> release)
    //    {
    //        _target = target;
    //        _release = release;
    //    }

    //    public void DoMouseDown(LingoMouseEvent mouse) => _target.RaiseMouseDown(mouse);
    //    public void DoMouseUp(LingoMouseEvent mouse) => _target.RaiseMouseUp(mouse);
    //    public void DoMouseMove(LingoMouseEvent mouse) => _target.RaiseMouseMove(mouse);
    //    public void DoMouseWheel(LingoMouseEvent mouse) => _target.RaiseMouseWheel(mouse);

    //    public void Release()
    //    {
    //        _release(this);
    //    }
    //}
}
