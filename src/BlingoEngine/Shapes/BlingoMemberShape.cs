using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using System.Threading.Tasks;

namespace BlingoEngine.Shapes
{
    /// <summary>
    /// Represents a vector shape cast member.
    /// </summary>
    public class BlingoMemberShape : BlingoMember, IBlingoMemberWithTexture
    {
        private readonly IBlingoFrameworkMemberShape _framework;

        public BlingoList<APoint> VertexList => _framework.VertexList;

        public BlingoShapeType ShapeType
        {
            get => _framework.ShapeType;
            set
            {
                if (_framework.ShapeType == value)
                    return;
                _framework.ShapeType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShapeTypeInt));
            }
        }

        public int ShapeTypeInt
        {
            get => (int)ShapeType;
            set => ShapeType = (BlingoShapeType)value;
        }

        public AColor FillColor
        {
            get => _framework.FillColor;
            set
            {
                if (_framework.FillColor == value)
                    return;
                _framework.FillColor = value;
                OnPropertyChanged();
            }
        }

        public AColor EndColor
        {
            get => _framework.EndColor;
            set
            {
                if (_framework.EndColor == value)
                    return;
                _framework.EndColor = value;
                OnPropertyChanged();
            }
        }

        public AColor StrokeColor
        {
            get => _framework.StrokeColor;
            set
            {
                if (_framework.StrokeColor == value)
                    return;
                _framework.StrokeColor = value;
                OnPropertyChanged();
            }
        }

        public int StrokeWidth
        {
            get => _framework.StrokeWidth;
            set
            {
                if (_framework.StrokeWidth == value)
                    return;
                _framework.StrokeWidth = value;
                OnPropertyChanged();
            }
        }

        public bool Closed
        {
            get => _framework.Closed;
            set
            {
                if (_framework.Closed == value)
                    return;
                _framework.Closed = value;
                OnPropertyChanged();
            }
        }

        public bool AntiAlias
        {
            get => _framework.AntiAlias;
            set
            {
                if (_framework.AntiAlias == value)
                    return;
                _framework.AntiAlias = value;
                OnPropertyChanged();
            }
        }

        public override int Width
        {
            get => Convert.ToInt32(_framework.Width);
            set
            {
                if (Convert.ToInt32(_framework.Width) == value)
                    return;
                _framework.Width = value;
                base.Width = value;
                OnPropertyChanged();
            }
        }

        public override int Height
        {
            get => Convert.ToInt32(_framework.Height);
            set
            {
                if (Convert.ToInt32(_framework.Height) == value)
                    return;
                _framework.Height = value;
                base.Height = value;
                OnPropertyChanged();
            }
        }

        public bool Filled
        {
            get => _framework.Filled;
            set
            {
                if (_framework.Filled == value)
                    return;
                _framework.Filled = value;
                OnPropertyChanged();
            }
        }

        public IAbstTexture2D? TextureBlingo => _framework.TextureBlingo;

        public T Framework<T>() where T : IBlingoFrameworkMemberShape => (T)_framework;

        public BlingoMemberShape(BlingoCast cast, IBlingoFrameworkMemberShape framework, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(framework, BlingoMemberType.VectorShape, cast, numberInCast, name, fileName, regPoint)
        {
            _framework = framework;
            RegPoint = new APoint(0, 0);
        }

        protected override BlingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_framework has to be retrieved from the factory");
        }

        public override void Preload() => _framework.Preload();
        public override Task PreloadAsync() => _framework.PreloadAsync();

        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
           => _framework.RenderToTexture(ink, transparentColor);
    }
}

