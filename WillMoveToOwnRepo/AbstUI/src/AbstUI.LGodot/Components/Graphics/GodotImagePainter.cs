using AbstUI.Components.Graphics;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Texts;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Godot;

namespace AbstUI.LGodot.Components.Graphics;

public partial class GodotImagePainter : IAbstImagePainter
{
    private readonly AbstGodotFontManager _fontManager;
    private readonly List<(Func<APoint?> GetTotalSize, Action<DrawingControl> DrawAction)> _drawActions = new();
    private readonly DrawingControl _control;
    private AColor? _clearColor;
    private bool _dirty;
    private string _name = string.Empty;
    private int _height;
    private int _width;

    public bool AutoResizeWidth { get; set; } = false;
    public bool AutoResizeHeight { get; set; } = true;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            _control.Name = value;
        }
    }
    public bool Pixilated
    {
        get => _control.TextureFilter == CanvasItem.TextureFilterEnum.Nearest;
        set => _control.TextureFilter = value ? CanvasItem.TextureFilterEnum.Nearest : CanvasItem.TextureFilterEnum.Linear;
    }

    public Control GodotControl => _control;

    public int Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            Resize(Width, value);
        }
    }
    public int Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            Resize(value, Height);
        }
    }

    public GodotImagePainter(AbstGodotFontManager fontManager, int width = 0, int height = 0)
    {
        _fontManager = fontManager;
        if (width == 0)
            AutoResizeWidth = true;
        if (height == 0) AutoResizeHeight = true;
        Width = width;
        Height = height;
        if (width == 0)
        {
            AutoResizeWidth = true;
            width = 10;
        }
        if (height == 0)
        {
            AutoResizeHeight = true;
            height = 10;
        }
        _control = new DrawingControl(() => _dirty = false)
        {

            Size = new Vector2(width, height),
            CustomMinimumSize = new Vector2(width, height)
        };
        _control.DrawActions = _drawActions;
    }

    public void Resize(int width, int height)
    {
        if (Width == width && Height == height)
            return;
        Width = width;
        Height = height;
        _control.Size = new Vector2(width, height);
        _control.CustomMinimumSize = _control.Size;
        MarkDirty();
    }

    public void Dispose()
    {
        _drawActions.Clear();
        _control.QueueFree();
    }

    private void MarkDirty()
    {
        _dirty = true;
        _control.QueueRedraw();
    }

    public void Render()
    {
        if (!_dirty) return;
        if (AutoResizeWidth || AutoResizeHeight)
        {
            int newWidth = Width;
            int newHeight = Height;
            foreach (var (getSize, _) in _drawActions)
            {
                var size = getSize?.Invoke();
                if (size != null)
                {
                    if (AutoResizeWidth && size.Value.X > newWidth)
                        newWidth = (int)size.Value.X;
                    if (AutoResizeHeight && size.Value.Y > newHeight)
                        newHeight = (int)size.Value.Y;
                }
            }
            if (newWidth > Width || newHeight > Height)
                Resize(AutoResizeWidth ? Math.Max(Width, newWidth) : Width,
                       AutoResizeHeight ? Math.Max(Height, newHeight) : Height);
        }

        _control.QueueRedraw();
    }

    public IAbstTexture2D GetTexture(string? name = null)
    {
        Render();
        var texture = _control.CreateAbstTexture2(name ?? Name);
        texture.DebugWriteToDiskInc();
        return texture;
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        _control.ClearColorProxy = color.ToGodotColor();
        MarkDirty();
    }

    public void SetPixel(APoint point, AColor color)
    {
        var p = point;
        var c = color.ToGodotColor();
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint(p.X + 1, p.Y + 1) : null,
            (control) => control.DrawRect(new Rect2(p.X, p.Y, 1, 1), c, true)));
        MarkDirty();
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start; var e = end; var c = color.ToGodotColor();
        _drawActions.Add((
            () =>
            {
                if (!AutoResizeWidth && !AutoResizeHeight) return null;
                int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
                int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
                return new APoint(maxX, maxY);
            },
             (control) => control.DrawLine(s.ToVector2(), e.ToVector2(), c, width)));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect; var c = color.ToGodotColor();
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint(r.Left + r.Width, r.Top + r.Height) : null,
            control =>
            {
                var godotRect = r.ToRect2();
                if (filled)
                    control.DrawRect(godotRect, c, true);
                else
                    control.DrawRect(godotRect, c, false, width);
            }
        ));
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var ctr = center; var c = color.ToGodotColor();
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint((int)(ctr.X + radius + 1), (int)(ctr.Y + radius + 1)) : null,
            control =>
            {
                if (filled)
                    control.DrawCircle(ctr.ToVector2(), radius, c);
                else
                    control.DrawArc(ctr.ToVector2(), radius, 0, 360, 32, c, width);
            }
        ));
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var ctr = center; var c = color.ToGodotColor();
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint((int)(ctr.X + radius + 1), (int)(ctr.Y + radius + 1)) : null,
             (control) => control.DrawArc(ctr.ToVector2(), radius, startDeg, endDeg, segments, c, width)));
        MarkDirty();
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        var pts = points.Select(p => p).ToArray();
        var c = color.ToGodotColor();
        _drawActions.Add((
            () =>
            {
                if (!AutoResizeWidth && !AutoResizeHeight) return null;
                int maxX = 0, maxY = 0;
                foreach (var p in pts)
                {
                    if (p.X > maxX) maxX = (int)p.X;
                    if (p.Y > maxY) maxY = (int)p.Y;
                }
                return new APoint(maxX + 1, maxY + 1);
            },
            control =>
            {
                var arr = pts.Select(p => p.ToVector2()).ToArray();
                if (filled)
                    control.DrawPolygon(arr, new[] { c });
                else
                    control.DrawPolyline(arr, c, width, true);
            }
        ));
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
        int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var col = (color ?? new AColor(0, 0, 0, 255)).ToGodotColor();
        var fontGodot = _fontManager.Get<FontFile>(font ?? string.Empty, style) ?? ThemeDB.FallbackFont;
        _drawActions.Add((
            () =>
            {
                if ((!AutoResizeWidth && !AutoResizeHeight) || string.IsNullOrEmpty(txt)) return null;
                if (!txt.Contains('\n'))
                {
                    float measuredW = fontGodot.GetStringSize(txt, alignment.ToGodot(), width, fontSize).X;
                    float w = width >= 0 ? MathF.Max(width, measuredW) : measuredW;
                    float h = fontGodot.GetHeight(fontSize);
                    return new APoint((int)(pos.X + w), (int)(pos.Y + h));
                }
                else
                {
                    var lines = txt.Split('\n');
                    float lineHeight = fontGodot.GetHeight(fontSize);
                    float maxW = 0;
                    foreach (var line in lines)
                        maxW = MathF.Max(maxW, fontGodot.GetStringSize(line, alignment.ToGodot(), width, fontSize).X);
                    float w = width >= 0 ? MathF.Max(width, maxW) : maxW;
                    float h = lineHeight * lines.Length;
                    return new APoint((int)(pos.X + w), (int)(pos.Y + h));
                }
            },
            control =>
            {
                if (!txt.Contains('\n'))
                {
                    int wBox = width >= 0 ? width : -1;
                    var p = new Vector2(pos.X, pos.Y + fontGodot.GetAscent(fontSize));
                    control.DrawString(fontGodot, p, txt, alignment.ToGodot(), wBox, fontSize, col);
                }
                else
                {
                    var lines = txt.Split('\n');
                    float lineHeight = fontGodot.GetHeight(fontSize);
                    float ascent = fontGodot.GetAscent(fontSize);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var p = new Vector2(pos.X, pos.Y + ascent + i * lineHeight);
                        int wBox = width >= 0 ? width : -1;
                        control.DrawString(fontGodot, p, lines[i], alignment.ToGodot(), wBox, fontSize, col);
                    }
                }
            }
        ));
        MarkDirty();
    }

    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
        int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left,
        AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var col = (color ?? new AColor(0, 0, 0, 255)).ToGodotColor();
        var fontGodot = _fontManager.Get<FontFile>(font ?? string.Empty, style) ?? ThemeDB.FallbackFont;
        _drawActions.Add((
            () =>
            {
                if ((!AutoResizeWidth && !AutoResizeHeight) || string.IsNullOrEmpty(txt)) return null;
                float measuredW = fontGodot.GetStringSize(txt, alignment.ToGodot(), width, fontSize).X;
                float w = width >= 0 ? MathF.Max(width, measuredW) : measuredW;
                float h = height >= 0 ? height : fontGodot.GetHeight(fontSize);
                return new APoint((int)(pos.X + w), (int)(pos.Y + h));
            },
            control =>
            {
                int wBox = width >= 0 ? width : -1;
                var p = new Vector2(pos.X, pos.Y + fontGodot.GetAscent(fontSize));
                control.DrawString(fontGodot, p, txt, alignment.ToGodot(), wBox, fontSize, col);
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        using var img = Image.CreateFromData(width, height, false, format.ToGodotFormat(), data);
        var tex = ImageTexture.CreateFromImage(img);
        if (tex == null) return;
        var pos = position;
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint((int)(pos.X + width), (int)(pos.Y + height)) : null,
            control =>
            {
                control.DrawTexture(tex, pos.ToVector2());
                tex.Dispose();
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        var tex = ((AbstGodotTexture2D)texture).Texture;
        var pos = position;
        _drawActions.Add((
            () => (AutoResizeWidth || AutoResizeHeight) ? new APoint((int)(pos.X + width), (int)(pos.Y + height)) : null,
             (control) => control.DrawTextureRect(tex, new Rect2(pos.X, pos.Y, width, height), false)));
        MarkDirty();
    }

    private sealed partial class DrawingControl : Control
    {
        public List<(Func<APoint?> GetTotalSize, Action<DrawingControl> DrawAction)> DrawActions = new();
        private readonly Action _resetDirty;

        public Color? ClearColorProxy { get; set; }
        public DrawingControl(Action resetDirty)
        {
            _resetDirty = resetDirty;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Draw()
        {
            if (ClearColorProxy.HasValue)
            {
                DrawRect(new Rect2(0, 0, Size.X, Size.Y), ClearColorProxy.Value, true);
            }
            foreach (var action in DrawActions)
                action.DrawAction(this);
            _resetDirty();

        }

        private static Node? _holder;
        private static SubViewport? _scratchVp;
        public AbstGodotTexture2D CreateAbstTexture2(string? name = null)
        {
            var control = this;
            var sizeI = new Vector2I((int)control.Size.X, (int)control.Size.Y);

            var holder = new Node { Name = "__tmp_vp_holder" + name };
            var tree = Engine.GetMainLoop() as SceneTree ?? throw new InvalidOperationException("No SceneTree available.");
            if (tree.Root.IsNodeReady())
                tree.Root.AddChild(holder);
            else
                tree.Root.CallDeferred(MethodName.AddChild, holder);

            var vp = new SubViewport
            {
                Disable3D = true,
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Once,
                Size = sizeI
            };
            holder.AddChild(vp);

            // copy draw snapshot into the clone
            var clone = new DrawingControl(() => { });
            clone.Position = Vector2.Zero;
            clone.Size = control.Size;
            clone.CustomMinimumSize = control.CustomMinimumSize;
            clone.DrawActions = new(control.DrawActions);
            clone.Visible = true;
            clone.QueueRedraw();
            vp.AddChild(clone);


            //if (control is DrawingControl src && clone is DrawingControl dst)
            //{
            //    dst.ClearColorProxy = src.ClearColorProxy;
            //    // shallow copy is fine; actions are immutable closures over value-types/strings
            //    dst.DrawActions = [.. src.DrawActions];
            //    dst.QueueRedraw();
            //}

            RenderingServer.ForceDraw(true, 2);

            using var img = vp.GetTexture().GetImage();
            img.Convert(Image.Format.Rgba8);
            var tex = ImageTexture.CreateFromImage(img);

            clone.QueueFree();
            vp.QueueFree();
            holder.QueueFree();

            return new AbstGodotTexture2D(tex, name ?? $"{control.Name}_Snapshot");
        }
    }
}
