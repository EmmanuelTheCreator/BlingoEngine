using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface IAbstUIFrameworkGfxWrapPanel : IAbstUIFrameworkGfxLayoutNode
    {
        /// <summary>Orientation of child layout.</summary>
        AOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        APoint ItemMargin { get; set; }

        /// <summary>Adds a child node to the container.</summary>
        void AddItem(IAbstUIFrameworkGfxNode child);
        void RemoveItem(IAbstUIFrameworkGfxNode child);
        IEnumerable<IAbstUIFrameworkGfxNode> GetItems();
        IAbstUIFrameworkGfxNode? GetItem(int index);
        void RemoveAll();
    }
}
