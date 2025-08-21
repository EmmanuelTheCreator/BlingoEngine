using LingoEngine.Events;

namespace LingoEngine.Inputs.Events
{
    /// <summary>
    /// Has Key Down Event interface.
    /// </summary>
    public interface IHasKeyDownEvent
    {
        void KeyDown(LingoKeyEvent key);
    }
}

