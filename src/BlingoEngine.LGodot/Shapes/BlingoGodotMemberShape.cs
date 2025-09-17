using Godot;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Primitives;
using BlingoEngine.Shapes;
using BlingoEngine.Sprites;
using AbstUI.LGodot.Primitives;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Shapes
{
    public partial class BlingoGodotMemberShape : Node2D, IBlingoFrameworkMemberShape, IDisposable
    {
        private AColor _fillColor = AColor.FromRGB(255, 255, 255);
        private BlingoShapeType _shapeType = BlingoShapeType.Rectangle;
        private int _strokeWidth = 1;
        private BlingoList<APoint> _vertexList = new();
        private float width;
        private float height;

        public bool IsLoaded { get; private set; }
        public bool IsDirty { get; private set; } = true;
        public BlingoList<APoint> VertexList
        {
            get => _vertexList;
            set
            {
                _vertexList = value;
                IsDirty = true;
                IsLoaded = false;
            }
        }
        public BlingoShapeType ShapeType
        {
            get => _shapeType;
            set
            {
                _shapeType = value;
                IsDirty = true;
                IsLoaded = false;
            }
        }
        public AColor FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                IsDirty = true;
                IsLoaded = false;
            }
        }
        public AColor EndColor { get; set; } = AColor.FromRGB(255, 255, 255);
        public AColor StrokeColor { get; set; } = AColor.FromRGB(0, 0, 0);
        public int StrokeWidth { get => _strokeWidth; set => _strokeWidth = value; }
        public bool Closed { get; set; } = true;
        public bool AntiAlias { get; set; } = true;
        public float Width { get => width; set { width = value; } }
        public float Height { get => height; set => height = value; }
        public (int TL, int TR, int BR, int BL) CornerRadius { get; set; } = (5, 5, 5, 5);
        public bool Filled { get; set; } = true;

        public IAbstTexture2D? TextureBlingo => null;

        public virtual BlingoGodotMemberShape CloneForSpriteDraw()
        {
            var clone = new BlingoGodotMemberShape();
            clone.VertexList = VertexList;
            clone.ShapeType = ShapeType;
            clone.FillColor = FillColor;
            clone.EndColor = EndColor;
            clone.StrokeColor = StrokeColor;
            clone.Closed = Closed;
            clone.AntiAlias = AntiAlias;
            clone.Width = Width;
            clone.Height = Height;
            clone.CornerRadius = CornerRadius;
            return clone;

        }


        public override void _Draw()
        {
            if (!IsDirty) return;
            var fill = FillColor.ToGodotColor();
            var stroke = StrokeColor.ToGodotColor();

            switch (ShapeType)
            {
                case BlingoShapeType.Line:
                case BlingoShapeType.PolyLine:
                    if (VertexList.Count == 0)
                        return;

                    var points = VertexList.Select(v => v.ToVector2()).ToArray();
                    if (points.Length >= 2)
                    {
                        if (ShapeType == BlingoShapeType.PolyLine && Closed && points.Length >= 2 && ShapeType != BlingoShapeType.Oval)
                            DrawPolyline(points.Append(points[0]).ToArray(), stroke, StrokeWidth, AntiAlias);
                        else
                            DrawLine(points[0], points[1], stroke, StrokeWidth, AntiAlias);
                    }
                    else
                        throw new Exception("A line needs at least 2 points");
                    var size = GetSize();
                    Width = size.X;
                    Height = size.Y;
                    break;

                case BlingoShapeType.Rectangle:
                    DrawRect(new Rect2(0, 0, new Vector2(Width, Height)), fill, Filled);
                    break;

                case BlingoShapeType.Oval:
                    DrawCircle(new Vector2(Width / 2, Height / 2), Width, fill, Filled);
                    break;

                case BlingoShapeType.RoundRect:
                    var rect = new Rect2(0, 0, new Vector2(Width, Height));

                    var stylebox = new StyleBoxFlat
                    {
                        BgColor = Filled ? fill : Colors.Transparent,
                        CornerRadiusTopLeft = CornerRadius.TL,
                        CornerRadiusTopRight = CornerRadius.TR,
                        CornerRadiusBottomLeft = CornerRadius.BL,
                        CornerRadiusBottomRight = CornerRadius.BR,
                        BorderWidthLeft = StrokeWidth,
                        BorderColor = stroke
                    };
                    DrawStyleBox(stylebox, rect);
                    break;
            }



            IsDirty = false;
        }


        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }

        public void CopyToClipboard() { }
        public void Erase() { VertexList.Clear(); }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void Preload()
        {
            if (IsLoaded)
                return;
            IsLoaded = true;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }
        public void Unload() { IsLoaded = false; IsDirty = true; }
        public new void Dispose()
        {
            base.Dispose();
        }


        private Vector2 GetSize()
        {
            if (VertexList.Count == 0) return Vector2.Zero;
            var min = VertexList[0].ToVector2();
            var max = min;
            foreach (var pt in VertexList)
            {
                var v = pt.ToVector2();
                min = min.Min(v);
                max = max.Max(v);
            }
            return max - min;
        }
        private Vector2 CalcCenter(Vector2[] points) => (points.Aggregate(Vector2.Zero, (acc, v) => acc + v)) / points.Length;

        private float CalcRadius(Vector2[] points)
        {
            var center = CalcCenter(points);
            return points.Max(p => p.DistanceTo(center));
        }

        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
        {
            // TODO
            return null;
        }
        public bool IsPixelTransparent(int x, int y) => false; // TODO
    }
}

