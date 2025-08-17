using AbstUI.Inputs;
using LingoEngine.Inputs;

namespace LingoEngine.Events
{
    public class LingoKeyEvent : AbstKeyEvent
    {
        public new LingoKey AbstUIKey { get; }

        public LingoKeyEvent(LingoKey key, AbstKeyEventType type)
            : base(key, type)
        {
            AbstUIKey = key;
        }
    }
}

