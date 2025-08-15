
namespace LingoEngine.AbstUI.Inputs
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
        public AbstUIMouse Mouse { get; }
        public AbstUIMouseEventType Type { get; }
        public bool ContinuePropation { get; set; } = true;
        public float MouseH => Mouse.MouseH;
        public float MouseV => Mouse.MouseV;
        public float WheelDelta => Mouse.WheelDelta;

        public AbstUIMouseEvent(AbstUIMouse lingoMouse, AbstUIMouseEventType type)
        {
            Mouse = lingoMouse;
            Type = type;
        }
    }

    public interface IAbstUIMouseEventHandler
    {
        void RaiseMouseDown(AbstUIMouseEvent mouse);
        void RaiseMouseUp(AbstUIMouseEvent mouse);
        void RaiseMouseMove(AbstUIMouseEvent mouse);
        void RaiseMouseWheel(AbstUIMouseEvent mouse);
    }
    public interface IAbstUIMouseEventSubscription
    {
        void Release();
    }

    public class AbstUIMouseEventSubscription : IAbstUIMouseEventSubscription
    {
        private readonly IAbstUIMouseEventHandler _target;
        private readonly Action<AbstUIMouseEventSubscription> _release;

        public AbstUIMouseEventSubscription(IAbstUIMouseEventHandler target, Action<AbstUIMouseEventSubscription> release)
        {
            _target = target;
            _release = release;
        }

        public void DoMouseDown(AbstUIMouseEvent mouse) => _target.RaiseMouseDown(mouse);
        public void DoMouseUp(AbstUIMouseEvent mouse) => _target.RaiseMouseUp(mouse);
        public void DoMouseMove(AbstUIMouseEvent mouse) => _target.RaiseMouseMove(mouse);
        public void DoMouseWheel(AbstUIMouseEvent mouse) => _target.RaiseMouseWheel(mouse);

        public void Release()
        {
            _release(this);
        }
    }
}
