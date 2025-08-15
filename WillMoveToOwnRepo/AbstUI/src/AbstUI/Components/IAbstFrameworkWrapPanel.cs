using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface IAbstFrameworkWrapPanel : IAbstFrameworkLayoutNode
    {
        /// <summary>Orientation of child layout.</summary>
        AOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        APoint ItemMargin { get; set; }

        /// <summary>Adds a child node to the container.</summary>
        void AddItem(IAbstFrameworkNode child);
        void RemoveItem(IAbstFrameworkNode child);
        IEnumerable<IAbstFrameworkNode> GetItems();
        IAbstFrameworkNode? GetItem(int index);
        void RemoveAll();
    }
}
