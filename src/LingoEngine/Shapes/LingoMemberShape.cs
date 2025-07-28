using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Shapes
{
    /// <summary>
    /// Represents a vector shape cast member.
    /// </summary>
    public class LingoMemberShape : LingoMember
    {
        private readonly ILingoFrameworkMemberShape _framework;

        public LingoList<LingoPoint> VertexList => _framework.VertexList;
        public LingoShapeType ShapeType { get => _framework.ShapeType; set => _framework.ShapeType = value; }
        public int ShapeTypeInt { get => (int)ShapeType; set => ShapeType = (LingoShapeType)value; }
        public LingoColor FillColor { get => _framework.FillColor; set => _framework.FillColor = value; }
        public LingoColor EndColor { get => _framework.EndColor; set => _framework.EndColor = value; }
        public LingoColor StrokeColor { get => _framework.StrokeColor; set => _framework.StrokeColor = value; }
        public int StrokeWidth { get => _framework.StrokeWidth; set => _framework.StrokeWidth = value; }
        public bool Closed { get => _framework.Closed; set => _framework.Closed = value; }
        public bool AntiAlias { get => _framework.AntiAlias; set => _framework.AntiAlias = value; }
        public override int Width { get => Convert.ToInt32(_framework.Width); set => _framework.Width = value; }
        public override int Height { get => Convert.ToInt32(_framework.Height); set => _framework.Height = value; }
        public bool Filled { get => _framework.Filled; set => _framework.Filled = value; }

        public T Framework<T>() where T : ILingoFrameworkMemberShape => (T)_framework;

        public LingoMemberShape(LingoCast cast, ILingoFrameworkMemberShape framework, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(framework, LingoMemberType.VectorShape, cast, numberInCast, name, fileName, regPoint)
        {
            _framework = framework;
            RegPoint = new LingoPoint(0, 0);
        }

        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_framework has to be retrieved from the factory");
        }

        public override void Preload() => _framework.Preload();
    }
}
