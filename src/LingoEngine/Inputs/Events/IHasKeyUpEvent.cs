using LingoEngine.Events;

namespace LingoEngine.Inputs.Events
{
    /// <summary>
    /// Has Key Up Event interface.
    /// </summary>
    public interface IHasKeyUpEvent
    {
        void KeyUp(LingoKeyEvent key);
    }
}

