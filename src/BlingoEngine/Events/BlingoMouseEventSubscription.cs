using AbstUI.Inputs;
using BlingoEngine.Inputs;

namespace BlingoEngine.Events
{

    public class BlingoMouseEvent : AbstMouseEvent
    {
        public new BlingoMouse Mouse { get; }

        public BlingoMouseEvent(BlingoMouse blingoMouse, AbstMouseEventType type)
            : base(blingoMouse, type)
        {
            Mouse = blingoMouse;
        }
        public BlingoMouseEvent(AbstMouseEvent mouseEvent, BlingoMouse mouse)
            : base(mouse, mouseEvent.Type)
        {
            Mouse = mouse;
            Type = mouseEvent.Type;
            ContinuePropagation = mouseEvent.ContinuePropagation;

        }
    }

    /// <summary>
    /// Lingo Mouse Event Handler interface.
    /// </summary>
    public interface IBlingoMouseEventHandler : IAbstMouseEventHandler<BlingoMouseEvent>
    {
    }
    /// <summary>
    /// Lingo Mouse Event Subscription interface.
    /// </summary>
    public interface IBlingoMouseEventSubscription
    {
        void Release();
    }

    //public class BlingoMouseEventSubscription : IAbstUIMouseSubscription
    //{
    //    private readonly IBlingoMouseEventHandler _target;
    //    private readonly Action<BlingoMouseEventSubscription> _release;

    //    public BlingoMouseEventSubscription(IBlingoMouseEventHandler target, Action<BlingoMouseEventSubscription> release)
    //    {
    //        _target = target;
    //        _release = release;
    //    }

    //    public void DoMouseDown(BlingoMouseEvent mouse) => _target.RaiseMouseDown(mouse);
    //    public void DoMouseUp(BlingoMouseEvent mouse) => _target.RaiseMouseUp(mouse);
    //    public void DoMouseMove(BlingoMouseEvent mouse) => _target.RaiseMouseMove(mouse);
    //    public void DoMouseWheel(BlingoMouseEvent mouse) => _target.RaiseMouseWheel(mouse);

    //    public void Release()
    //    {
    //        _release(this);
    //    }
    //}
}

