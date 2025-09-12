using AbstUI.Primitives;

namespace AbstUI.Components
{
    public interface IAbstFrameworkNode : IDisposable
    {
        /// <summary>Name of the node.</summary>
        string Name { get; set; }
        bool Visibility { get; set; }
        float Width { get; set; }
        float Height { get; set; }

        /// <summary>Margin around the node.</summary>
        AMargin Margin { get; set; }
        int ZIndex { get; set; }
        object FrameworkNode { get; }
    }
    /// <summary>
    /// Basic framework object that can be positioned and sized on screen.
    /// </summary>
    public interface IAbstFrameworkLayoutNode : IAbstFrameworkNode
    {

        float X { get; set; }
        float Y { get; set; }

    }

}
