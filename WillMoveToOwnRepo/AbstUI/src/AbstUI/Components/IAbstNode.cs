using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface IAbstNode
    {
        string Name { get; set; }
        bool Visibility { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        AMargin Margin { get; set; }
        T Framework<T>() where T : IAbstFrameworkNode;
        IAbstFrameworkNode FrameworkObj { get; set; }
    }
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface IAbstLayoutNode : IAbstNode
    {
        float X { get; set; }
        float Y { get; set; }

    }
}
