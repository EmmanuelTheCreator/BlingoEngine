using LingoEngine.AbstUI.Primitives;
using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Shapes
{
    public interface ILingoFrameworkMemberShape : ILingoFrameworkMemberWithTexture
    {
        LingoList<APoint> VertexList { get; }
        LingoShapeType ShapeType { get; set; }
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
