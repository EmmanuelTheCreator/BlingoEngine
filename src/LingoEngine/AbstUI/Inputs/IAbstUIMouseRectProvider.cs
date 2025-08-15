using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.AbstUI.Inputs
{
    /// <summary>
    /// Supplies the mouse region and activation state for a proxy mouse.
    /// </summary>
    public interface IAbstUIMouseRectProvider : IAbstUIActivationProvider
    {
        /// <summary>
        /// Rectangle describing the area in which mouse events are valid.
        /// </summary>
        ARect MouseOffset { get; }
    }
}

