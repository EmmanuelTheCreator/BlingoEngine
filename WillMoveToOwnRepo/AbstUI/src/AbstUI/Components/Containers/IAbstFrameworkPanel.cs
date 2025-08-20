using AbstUI.Primitives;

namespace AbstUI.Components.Containers
{
    /// <summary>
    /// Framework specific container allowing absolute positioning of child nodes.
    /// </summary>

    public interface IAbstFrameworkPanel : IAbstFrameworkLayoutNode
    {
        /// <summary>Adds a child node to the panel.</summary>
        void AddItem(IAbstFrameworkLayoutNode child);
        void RemoveItem(IAbstFrameworkLayoutNode child);
        void RemoveAll();
        IEnumerable<IAbstFrameworkLayoutNode> GetItems();

        /// <summary>Background color of the panel.</summary>
        AColor? BackgroundColor { get; set; }
        /// <summary>Border color of the panel.</summary>
        AColor? BorderColor { get; set; }
        /// <summary>Border width around the panel.</summary>
        float BorderWidth { get; set; }
    }
    public interface IAbstFrameworkLayoutWrapper : IAbstFrameworkLayoutNode
    {

    }
}
