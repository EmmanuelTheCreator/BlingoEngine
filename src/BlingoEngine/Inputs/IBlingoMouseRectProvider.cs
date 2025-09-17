using AbstUI.Primitives;

namespace BlingoEngine.Inputs
{
    /// <summary>
    /// Supplies the mouse region and activation state for a proxy mouse.
    /// </summary>
    public interface IBlingoMouseRectProvider : IBlingoActivationProvider
    {
        /// <summary>
        /// Rectangle describing the area in which mouse events are valid.
        /// </summary>
        ARect MouseOffset { get; }
    }
}


