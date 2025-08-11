using LingoEngine.Inputs;

namespace LingoEngine.Events
{
    public enum LingoMouseEventType
    {
        MouseUp,MouseDown, MouseMove
    }
    public class LingoMouseEvent
    {
        public LingoMouse Mouse { get; }
        public LingoMouseEventType Type { get; }
        public bool ContinuePropation { get; set; } = true;
        public float MouseH => Mouse.MouseH;
        public float MouseV => Mouse.MouseV;

        public LingoMouseEvent(LingoMouse lingoMouse, LingoMouseEventType type)
        {
            Mouse = lingoMouse;
            Type = type;
        }
    }

    public interface ILingoMouseEventHandler
    {
        void RaiseMouseDown(LingoMouseEvent mouse);
        void RaiseMouseUp(LingoMouseEvent mouse);
        void RaiseMouseMove(LingoMouseEvent mouse);
    }
    public interface ILingoMouseEventSubscription
    {
        void Release();
    }

    public class LingoMouseEventSubscription : ILingoMouseEventSubscription
    {
        private readonly ILingoMouseEventHandler _target;
        private readonly Action<LingoMouseEventSubscription> _release;

        public LingoMouseEventSubscription(ILingoMouseEventHandler target, Action<LingoMouseEventSubscription> release)
        {
            _target = target;
            _release = release;
        }

        public void DoMouseDown(LingoMouseEvent mouse) => _target.RaiseMouseDown(mouse);
        public void DoMouseUp(LingoMouseEvent mouse) => _target.RaiseMouseUp(mouse);
        public void DoMouseMove(LingoMouseEvent mouse) => _target.RaiseMouseMove(mouse);

        public void Release()
        {
            _release(this);
        }
    }
}
