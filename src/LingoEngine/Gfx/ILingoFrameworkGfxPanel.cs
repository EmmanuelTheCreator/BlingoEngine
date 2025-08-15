using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific container allowing absolute positioning of child nodes.
    /// </summary>

    public interface ILingoFrameworkGfxPanel : ILingoFrameworkGfxLayoutNode
    {
        /// <summary>Adds a child node to the panel.</summary>
        void AddItem(ILingoFrameworkGfxLayoutNode child);
        void RemoveItem(ILingoFrameworkGfxLayoutNode child);
        void RemoveAll();
        IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems();

        /// <summary>Background color of the panel.</summary>
        AColor? BackgroundColor { get; set; }
        /// <summary>Border color of the panel.</summary>
        AColor? BorderColor { get; set; }
        /// <summary>Border width around the panel.</summary>
        float BorderWidth { get; set; }
    }
    public interface ILingoFrameworkGfxLayoutWrapper : ILingoFrameworkGfxLayoutNode
    {

    }
}
