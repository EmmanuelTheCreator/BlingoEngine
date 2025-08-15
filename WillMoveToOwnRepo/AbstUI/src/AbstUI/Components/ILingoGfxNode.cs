using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface IAbstUIGfxNode
    {
        string Name { get; set; }
        bool Visibility { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        AMargin Margin { get; set; }
        T Framework<T>() where T : IAbstUIFrameworkGfxNode;
        IAbstUIFrameworkGfxNode FrameworkObj { get; }
    }
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface IAbstUIGfxLayoutNode : IAbstUIGfxNode
    {
        float X { get; set; }
        float Y { get; set; }

    }
}
