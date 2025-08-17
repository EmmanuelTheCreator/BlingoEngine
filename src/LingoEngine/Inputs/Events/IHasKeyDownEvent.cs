using LingoEngine.Events;

namespace LingoEngine.Inputs.Events
{
    public interface IHasKeyDownEvent
    {
        void KeyDown(LingoKeyEvent key);
    }
}

