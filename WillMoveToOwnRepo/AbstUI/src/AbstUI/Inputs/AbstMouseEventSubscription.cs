
namespace AbstUI.Inputs
{
    public enum AbstMouseEventType
    {
        MouseUp,
        MouseDown,
        MouseMove,
        MouseWheel
    }
    public class AbstMouseEvent
    {
        public IAbstMouse Mouse { get; }
        public AbstMouseEventType Type { get; }
        public bool ContinuePropation { get; set; } = true;
        public float MouseH => Mouse.MouseH;
        public float MouseV => Mouse.MouseV;
        public float WheelDelta => Mouse.WheelDelta;

        public AbstMouseEvent(IAbstMouse lingoMouse, AbstMouseEventType type)
        {
            Mouse = lingoMouse;
            Type = type;
        }
    }

    public interface IAbstMouseEventHandler<TAbstUIMouseEvent> 
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        void RaiseMouseDown(TAbstUIMouseEvent mouse);
        void RaiseMouseUp(TAbstUIMouseEvent mouse);
        void RaiseMouseMove(TAbstUIMouseEvent mouse);
        void RaiseMouseWheel(TAbstUIMouseEvent mouse);
    }
    public interface IAbstMouseEventSubscription
    {
        void Release();
    }

    public class AbstMouseEventSubscription<TAbstUIMouseEvent> : IAbstMouseEventSubscription
        where TAbstUIMouseEvent : AbstMouseEvent
    {
        private readonly IAbstMouseEventHandler<TAbstUIMouseEvent> _target;
        private readonly Action<AbstMouseEventSubscription<TAbstUIMouseEvent>> _release;

        public AbstMouseEventSubscription(IAbstMouseEventHandler<TAbstUIMouseEvent> target, Action<AbstMouseEventSubscription<TAbstUIMouseEvent>> release)
        {
            _target = target;
            _release = release;
        }

        public void DoMouseDown(TAbstUIMouseEvent mouse) => _target.RaiseMouseDown(mouse);
        public void DoMouseUp(TAbstUIMouseEvent mouse) => _target.RaiseMouseUp(mouse);
        public void DoMouseMove(TAbstUIMouseEvent mouse) => _target.RaiseMouseMove(mouse);
        public void DoMouseWheel(TAbstUIMouseEvent mouse) => _target.RaiseMouseWheel(mouse);

        public void Release()
        {
            _release(this);
        }
    }
}
