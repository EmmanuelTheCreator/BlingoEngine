using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface ILingoFrameworkGfxWrapPanel : ILingoFrameworkGfxLayoutNode
    {
        /// <summary>Orientation of child layout.</summary>
        AOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        APoint ItemMargin { get; set; }

        /// <summary>Adds a child node to the container.</summary>
        void AddItem(ILingoFrameworkGfxNode child);
        void RemoveItem(ILingoFrameworkGfxNode child);
        IEnumerable<ILingoFrameworkGfxNode> GetItems();
        ILingoFrameworkGfxNode? GetItem(int index);
        void RemoveAll();
    }
}
