using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Shapes
{
    public interface ILingoFrameworkMemberShape : ILingoFrameworkMember
    {
        LingoList<LingoPoint> VertexList { get; }
        LingoShapeType ShapeType { get; set; }
        LingoColor FillColor { get; set; }
        LingoColor EndColor { get; set; }
        LingoColor StrokeColor { get; set; }
        int StrokeWidth { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Closed { get; set; }
        bool AntiAlias { get; set; }
        bool Filled { get; set; }
    }
}
