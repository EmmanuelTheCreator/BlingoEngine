using AbstUI.Inputs;
using BlingoEngine.Inputs;

namespace BlingoEngine.Events
{
    public class BlingoKeyEvent : AbstKeyEvent
    {
        public new BlingoKey AbstUIKey { get; }

        public BlingoKeyEvent(BlingoKey key, AbstKeyEventType type)
            : base(key, type)
        {
            AbstUIKey = key;
        }
    }
}


