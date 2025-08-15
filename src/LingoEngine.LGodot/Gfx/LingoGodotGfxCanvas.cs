using Godot;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Styles;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Texts;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Texts;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstUIFrameworkGfxCanvas"/>.
    /// </summary>
    public partial class LingoGodotGfxCanvas : Control, IAbstUIFrameworkGfxCanvas, IDisposable
    {
        private readonly ILingoFontManager _fontManager;
        private AMargin _margin = AMargin.Zero;
        private float _desiredWidth = 0;
        private float _desiredHeight = 0;
        private Color? _clearColor;
        private bool _dirty;
        private readonly List<Action> _drawActions = new();
        public LingoGodotGfxCanvas(AbstUIGfxCanvas canvas, ILingoFontManager fontManager, int width, int height)
        {
            _fontManager = fontManager;
            canvas.Init(this);
            Size = new Vector2(width, height);
            MouseFilter = MouseFilterEnum.Ignore;
            //TextureFilter = TextureFilterEnum.Nearest; // Use nearest neighbor for pixel art style
        }
        public bool Pixilated
        {
            get => TextureFilter == TextureFilterEnum.Nearest;
            set => TextureFilter = value ? TextureFilterEnum.Nearest : TextureFilterEnum.Linear;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = Size; _desiredWidth = value; } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value);CustomMinimumSize = Size; _desiredHeight = value; } }
        public bool Visibility { get => Visible; set => Visible = value; }
        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        string IAbstUIFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public object FrameworkNode => this;
        private void MarkDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                //Console.WriteLine(Name + ":MarkDirty()");
                QueueRedraw();
            }
        }

        public override void _Draw()
        {
            //Console.WriteLine(Name + ":_Draw()");
            if (_clearColor.HasValue)
            {
                DrawRect(new Rect2(0, 0, _desiredWidth, _desiredHeight), _clearColor.Value, true);
                Size = new Vector2(_desiredWidth, _desiredHeight);
                CustomMinimumSize = Size; // Set the minimum size to the desired size
            }
            foreach (var drawAction in _drawActions)
                drawAction();

            // Keep the draw actions so that the canvas can redraw itself when
            // becoming visible again (e.g. when switching tabs).  They will be
            // cleared when a new Clear() call is issued via <see cref="Clear"/>.
            _dirty = false;
        }

        public void Clear(AColor color)
        {
            _drawActions.Clear();
            _clearColor = color.ToGodotColor();
            MarkDirty();
        }

        public void SetPixel(APoint point, AColor color)
        {
            _drawActions.Add(() => DrawRect(new Rect2(point.X, point.Y, 1, 1), color.ToGodotColor(), true));
            MarkDirty();
        }

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        {
            _drawActions.Add(() => base.DrawLine(start.ToVector2(), end.ToVector2(), color.ToGodotColor(), width));
            MarkDirty();
        }

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        {
            var godotRect = rect.ToRect2();
            var godotColor = color.ToGodotColor();

            if (filled)
                _drawActions.Add(() => DrawRect(godotRect, godotColor, true));
            else
                _drawActions.Add(() => DrawRect(godotRect, godotColor, false, width));
            MarkDirty();
        }

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        {
            if (filled)
                _drawActions.Add(() => base.DrawCircle(center.ToVector2(), radius, color.ToGodotColor()));
            else
                _drawActions.Add(() => DrawArc(center.ToVector2(), radius, 0, 360, 32, color.ToGodotColor(), width));

            MarkDirty();
        }

        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        {
            _drawActions.Add(() => DrawArc(center.ToVector2(), radius, startDeg, endDeg, segments, color.ToGodotColor(), width));
            MarkDirty();
        }

        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        {
            var arr = points.Select(p => p.ToVector2()).ToArray();
          
            if (filled)
                _drawActions.Add(() => DrawPolygon(arr, [color.ToGodotColor()]));
            else
                _drawActions.Add(() => DrawPolyline(arr, color.ToGodotColor(), width, true));
            MarkDirty();
        }

        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstUITextAlignment alignment = AbstUITextAlignment.Left)
        {
            Font fontGodot = _fontManager.Get<FontFile>(font ?? "") ?? ThemeDB.FallbackFont;
            Color col = color.HasValue ? color.Value.ToGodotColor() : Colors.Black;

            if (!text.Contains('\n'))
            {
                int w = width >= 0 ? width : -1;
                _drawActions.Add(() => DrawString(fontGodot, position.ToVector2(), text, alignment.ToGodot(), w, fontSize, col));
            }
            else
            {
                var lines = text.Split('\n');
                _drawActions.Add(() =>
                {
                    var lineHeight = fontGodot.GetHeight();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Vector2 pos = new Vector2(position.X, position.Y + i * lineHeight);
                        int w = width >= 0 ? width : -1;
                        DrawString(fontGodot, pos, lines[i], alignment.ToGodot(), w, fontSize, col);
                    }
                });
            }

            MarkDirty();
        }


        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        {
            //DrawRect(LingoRect.New(position.X, position.Y, width, height),new LingoColor(255,0,0));
            var img = Image.CreateFromData(width, height, false, format.ToGodotFormat(), data);
            var tex = ImageTexture.CreateFromImage(img);
            if (tex == null) return;
            _drawActions.Add(() =>
            {
                DrawTexture(tex, position.ToVector2());
                tex.Dispose();
            });
            MarkDirty();
        } 
        
        public void DrawPicture(IAbstUITexture2D texture, int width, int height, APoint position)
        {
            var tex = ((LingoGodotTexture2D)texture).Texture;    
            _drawActions.Add(() =>
            {
                DrawTextureRect(tex, new Rect2(position.X, position.Y, width, height), false); // don't tile
            });
            MarkDirty();
        }

        public new void Dispose()
        {
            _drawActions.Clear();
            QueueFree();
            base.Dispose();
        }
    }
}
