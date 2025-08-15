using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific container allowing absolute positioning of child nodes.
    /// </summary>

    public interface IAbstUIFrameworkGfxPanel : IAbstUIFrameworkGfxLayoutNode
    {
        /// <summary>Adds a child node to the panel.</summary>
        void AddItem(IAbstUIFrameworkGfxLayoutNode child);
        void RemoveItem(IAbstUIFrameworkGfxLayoutNode child);
        void RemoveAll();
        IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetItems();

        /// <summary>Background color of the panel.</summary>
        AColor? BackgroundColor { get; set; }
        /// <summary>Border color of the panel.</summary>
        AColor? BorderColor { get; set; }
        /// <summary>Border width around the panel.</summary>
        float BorderWidth { get; set; }
    }
    public interface IAbstUIFrameworkGfxLayoutWrapper : IAbstUIFrameworkGfxLayoutNode
    {

    }
}
