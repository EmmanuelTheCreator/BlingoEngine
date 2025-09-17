using AbstUI.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Primitives;

namespace BlingoEngine.Shapes
{
    /// <summary>
    /// Lingo Framework Member Shape interface.
    /// </summary>
    public interface IBlingoFrameworkMemberShape : IBlingoFrameworkMemberWithTexture
    {
        BlingoList<APoint> VertexList { get; }
        BlingoShapeType ShapeType { get; set; }
        AColor FillColor { get; set; }
        AColor EndColor { get; set; }
        AColor StrokeColor { get; set; }
        int StrokeWidth { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Closed { get; set; }
        bool AntiAlias { get; set; }
        bool Filled { get; set; }
    }
}

