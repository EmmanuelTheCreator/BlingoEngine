
namespace AbstUI.Inputs
{
    public enum AbstUIMouseEventType
    {
        MouseUp,
        MouseDown,
        MouseMove,
        MouseWheel
    }
    public class AbstUIMouseEvent
    {
        public IAbstUIMouse Mouse { get; }
        public AbstUIMouseEventType Type { get; }
        public bool ContinuePropation { get; set; } = true;
        public float MouseH => Mouse.MouseH;
        public float MouseV => Mouse.MouseV;
        public float WheelDelta => Mouse.WheelDelta;

        public AbstUIMouseEvent(IAbstUIMouse lingoMouse, AbstUIMouseEventType type)
        {
            Mouse = lingoMouse;
            Type = type;
        }
    }

    public interface IAbstUIMouseEventHandler<TAbstUIMouseEvent> 
        where TAbstUIMouseEvent : AbstUIMouseEvent
    {
        void RaiseMouseDown(TAbstUIMouseEvent mouse);
        void RaiseMouseUp(TAbstUIMouseEvent mouse);
        void RaiseMouseMove(TAbstUIMouseEvent mouse);
        void RaiseMouseWheel(TAbstUIMouseEvent mouse);
    }
    public interface IAbstUIMouseEventSubscription
    {
        void Release();
    }

    public class AbstUIMouseEventSubscription<TAbstUIMouseEvent> : IAbstUIMouseEventSubscription
        where TAbstUIMouseEvent : AbstUIMouseEvent
    {
        private readonly IAbstUIMouseEventHandler<TAbstUIMouseEvent> _target;
        private readonly Action<AbstUIMouseEventSubscription<TAbstUIMouseEvent>> _release;

        public AbstUIMouseEventSubscription(IAbstUIMouseEventHandler<TAbstUIMouseEvent> target, Action<AbstUIMouseEventSubscription<TAbstUIMouseEvent>> release)
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
