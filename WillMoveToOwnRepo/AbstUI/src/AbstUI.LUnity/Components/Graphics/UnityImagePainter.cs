using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.LUnity.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using UnityEngine;
using AbstUI.LUnity.Bitmaps;
using AbstUI.LUnity.Styles;

namespace AbstUI.LUnity.Components.Graphics;

public class UnityImagePainter : IAbstImagePainter
{
    private readonly UnityFontManager _fontManager;
    private Texture2D _texture;
    private readonly List<(Func<APoint?> GetTotalSize, Action<Texture2D> DrawAction)> _drawActions = new();
    private AColor? _clearColor;
    private bool _dirty;

    public int Width { get; set; }
    public int Height { get; set; }
    public bool Pixilated
    {
        get => _texture.filterMode == FilterMode.Point;
        set => _texture.filterMode = value ? FilterMode.Point : FilterMode.Bilinear;
    }
    public bool AutoResize { get; set; } = true;
    public Texture2D Texture => _texture;

    public UnityImagePainter(IAbstFontManager fontManager, int width = 0, int height = 0)
    {
        _fontManager = (UnityFontManager)fontManager;
        Width = width > 0 ? Math.Min(width, 4096) : 10;
        Height = height > 0 ? Math.Min(height, 4096) : 10;
        _texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        ClearTexture();
        _dirty = true;
    }

    private void ClearTexture()
    {
        var arr = new Color[Width * Height];
        for (int i = 0; i < arr.Length; i++) arr[i] = new Color(0, 0, 0, 0);
        _texture.SetPixels(arr);
        _texture.Apply();
    }

    public void Dispose()
    {
        if (_texture != null)
        {
            UnityEngine.Object.Destroy(_texture);
        }
    }

    private void MarkDirty() => _dirty = true;

    public void Render()
    {
        if (!_dirty) return;

        var newWidth = Width;
        var newHeight = Height;
        if (AutoResize)
        {
            foreach (var a in _drawActions)
            {
                var newSize = a.GetTotalSize();
                if (newSize != null && (newSize.Value.X > newWidth || newSize.Value.Y > newHeight))
                {
                    newWidth = (int)MathF.Max(newWidth, newSize.Value.X);
                    newHeight = (int)MathF.Max(newHeight, newSize.Value.Y);
                }
            }
        }
        newWidth = Math.Min(newWidth, 4096);
        newHeight = Math.Min(newHeight, 4096);
        if (newWidth > Width || newHeight > Height)
        {
            var nw = Math.Max(Width, newWidth);
            var nh = Math.Max(Height, newHeight);
            var newTex = new Texture2D(nw, nh, TextureFormat.RGBA32, false)
            {
                filterMode = _texture.filterMode
            };
            var clear = new Color(0, 0, 0, 0);
            var arr = new Color[nw * nh];
            for (int i = 0; i < arr.Length; i++) arr[i] = clear;
            newTex.SetPixels(arr);
            var oldPixels = _texture.GetPixels(0, 0, Width, Height);
            newTex.SetPixels(0, 0, Width, Height, oldPixels);
            UnityEngine.Object.Destroy(_texture);
            _texture = newTex;
            Width = nw;
            Height = nh;
        }

        if (_clearColor.HasValue)
        {
            var c = _clearColor.Value.ToUnityColor();
            var arr = new Color[Width * Height];
            for (int i = 0; i < arr.Length; i++) arr[i] = c;
            _texture.SetPixels(arr);
        }

        foreach (var action in _drawActions)
            action.DrawAction(_texture);

        _texture.Apply();
        _dirty = false;
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    private static void SetPixel(Texture2D tex, int x, int y, Color col)
    {
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return;
        tex.SetPixel(x, tex.height - 1 - y, col);
    }

    public void SetPixel(APoint point, AColor color)
    {
        var p = point; var c = color.ToUnityColor();
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)p.X + 1, (int)p.Y + 1) : null,
            tex => SetPixel(tex, (int)p.X, (int)p.Y, c)
        ));
        MarkDirty();
    }

    private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col, float width)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int w = Mathf.Max(1, Mathf.RoundToInt(width));
        while (true)
        {
            for (int wx = -w / 2; wx <= w / 2; wx++)
                for (int wy = -w / 2; wy <= w / 2; wy++)
                    SetPixel(tex, x0 + wx, y0 + wy, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start; var e = end; var col = color.ToUnityColor(); var w = width;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                float maxX = MathF.Max(s.X, e.X);
                float maxY = MathF.Max(s.Y, e.Y);
                return EnsureCapacity((int)maxX + 1, (int)maxY + 1);
            },
            tex => DrawLine(tex, (int)s.X, (int)s.Y, (int)e.X, (int)e.Y, col, w)
        ));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect; var col = color.ToUnityColor(); var w = width;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)r.X + r.Width, (int)r.Y + r.Height) : null,
            tex =>
            {
                int x0 = Mathf.RoundToInt(r.X);
                int y0 = Mathf.RoundToInt(r.Y);
                int x1 = x0 + r.Width;
                int y1 = y0 + r.Height;
                if (filled)
                {
                    for (int y = y0; y < y1; y++)
                        for (int x = x0; x < x1; x++)
                            SetPixel(tex, x, y, col);
                }
                else
                {
                    DrawLine(tex, x0, y0, x1 - 1, y0, col, w);
                    DrawLine(tex, x1 - 1, y0, x1 - 1, y1 - 1, col, w);
                    DrawLine(tex, x1 - 1, y1 - 1, x0, y1 - 1, col, w);
                    DrawLine(tex, x0, y1 - 1, x0, y0, col, w);
                }
            }
        ));
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var c = center; var rad = Mathf.RoundToInt(radius); var col = color.ToUnityColor(); var w = width; var f = filled;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)c.X + rad + 1, (int)c.Y + rad + 1) : null,
            tex =>
            {
                int cx = Mathf.RoundToInt(c.X);
                int cy = Mathf.RoundToInt(c.Y);
                for (int y = -rad; y <= rad; y++)
                {
                    int xx = (int)Mathf.Sqrt(rad * rad - y * y);
                    if (f)
                    {
                        for (int x = -xx; x <= xx; x++)
                            SetPixel(tex, cx + x, cy + y, col);
                    }
                    else
                    {
                        DrawLine(tex, cx - xx, cy + y, cx - xx, cy + y, col, w);
                        DrawLine(tex, cx + xx, cy + y, cx + xx, cy + y, col, w);
                    }
                }
            }
        ));
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var c = center; var rad = radius; var s = startDeg; var e = endDeg; var segs = segments; var col = color.ToUnityColor(); var w = width;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                return EnsureCapacity((int)c.X + (int)MathF.Ceiling(rad) + 1, (int)c.Y + (int)MathF.Ceiling(rad) + 1);
            },
            tex =>
            {
                float start = s * Mathf.Deg2Rad;
                float end = e * Mathf.Deg2Rad;
                float step = (end - start) / segs;
                int prevX = Mathf.RoundToInt(c.X + Mathf.Cos(start) * rad);
                int prevY = Mathf.RoundToInt(c.Y + Mathf.Sin(start) * rad);
                for (int i = 1; i <= segs; i++)
                {
                    float ang = start + step * i;
                    int nx = Mathf.RoundToInt(c.X + Mathf.Cos(ang) * rad);
                    int ny = Mathf.RoundToInt(c.Y + Mathf.Sin(ang) * rad);
                    DrawLine(tex, prevX, prevY, nx, ny, col, w);
                    prevX = nx; prevY = ny;
                }
            }
        ));
        MarkDirty();
    }

    private static bool PointInTriangle(int px, int py, APoint a, APoint b, APoint c)
    {
        float Area(APoint p1, APoint p2, APoint p3) =>
            (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y)) / 2f;
        float A = Math.Abs(Area(a, b, c));
        float A1 = Math.Abs(Area(new APoint(px, py), b, c));
        float A2 = Math.Abs(Area(a, new APoint(px, py), c));
        float A3 = Math.Abs(Area(a, b, new APoint(px, py)));
        return Math.Abs(A - (A1 + A2 + A3)) < 0.01f;
    }

    private static void FillTriangle(Texture2D tex, APoint p0, APoint p1, APoint p2, Color col)
    {
        int minX = Mathf.FloorToInt(Mathf.Min(p0.X, Mathf.Min(p1.X, p2.X)));
        int maxX = Mathf.CeilToInt(Mathf.Max(p0.X, Mathf.Max(p1.X, p2.X)));
        int minY = Mathf.FloorToInt(Mathf.Min(p0.Y, Mathf.Min(p1.Y, p2.Y)));
        int maxY = Mathf.CeilToInt(Mathf.Max(p0.Y, Mathf.Max(p1.Y, p2.Y)));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (PointInTriangle(x, y, p0, p1, p2))
                    SetPixel(tex, x, y, col);
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        if (points.Count < 2) return;
        var pts = new APoint[points.Count];
        for (int i = 0; i < points.Count; i++) pts[i] = points[i];
        var col = color.ToUnityColor(); var w = width; var f = filled;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                float maxX = pts.Max(p => p.X);
                float maxY = pts.Max(p => p.Y);
                return EnsureCapacity((int)maxX + 1, (int)maxY + 1);
            },
            tex =>
            {
                for (int i = 0; i < pts.Length - 1; i++)
                    DrawLine(tex, (int)pts[i].X, (int)pts[i].Y, (int)pts[i + 1].X, (int)pts[i + 1].Y, col, w);
                DrawLine(tex, (int)pts[^1].X, (int)pts[^1].Y, (int)pts[0].X, (int)pts[0].Y, col, w);
                if (f)
                {
                    var p0 = pts[0];
                    for (int i = 1; i < pts.Length - 1; i++)
                        FillTriangle(tex, p0, pts[i], pts[i + 1], col);
                }
            }
        ));
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position; var txt = text; var col = (color ?? new AColor(0, 0, 0)).ToUnityColor();
        var fnt = font; var fs = fontSize; var w = width; var align = alignment; var st = style;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                float textW = w >= 0 ? w : _fontManager.MeasureTextWidth(txt, fnt ?? string.Empty, fs);
                var fi = _fontManager.GetFontInfo(fnt ?? string.Empty, fs);
                return EnsureCapacity((int)(pos.X + textW), (int)(pos.Y + fi.Height));
            },
            tex =>
            {
                var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
                UnityEngine.Graphics.Blit(tex, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, tex.width, tex.height, 0);
                var style = new GUIStyle
                {
                    font = fnt != null ? _fontManager.Get<Font>(fnt, st) ?? _fontManager.GetDefaultFont<Font>() : _fontManager.GetDefaultFont<Font>(),
                    fontSize = fs,
                    normal = new GUIStyleState { textColor = col }
                };
                style.alignment = align switch
                {
                    AbstTextAlignment.Center => TextAnchor.MiddleCenter,
                    AbstTextAlignment.Right => TextAnchor.MiddleRight,
                    _ => TextAnchor.UpperLeft
                };
                var rect = new Rect(pos.X, pos.Y, w >= 0 ? w : tex.width, tex.height);
                GUI.Label(rect, txt, style);
                GL.PopMatrix();
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        var dat = data; var w = width; var h = height; var pos = position; var fmt = format;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)pos.X + w, (int)pos.Y + h) : null,
            tex =>
            {
                var t = new Texture2D(w, h, fmt.ToUnityFormat(), false);
                t.LoadRawTextureData(dat);
                t.Apply();
                var colors = t.GetPixels(0, 0, w, h);
                int dstX = Mathf.RoundToInt(pos.X);
                int dstY = tex.height - Mathf.RoundToInt(pos.Y) - h;
                tex.SetPixels(dstX, dstY, w, h, colors);
                UnityEngine.Object.Destroy(t);
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        var pos = position; var w = width; var h = height;
        switch (texture)
        {
            case UnityTexture2D ut when ut.Texture != null:
                var src = ut.Texture;
                _drawActions.Add((
                    () =>
                    {
                        if (!AutoResize) return null;
                        int copyW = Math.Min(w, src.width);
                        int copyH = Math.Min(h, src.height);
                        return EnsureCapacity((int)pos.X + copyW, (int)pos.Y + copyH);
                    },
                    tex =>
                    {
                        int copyW = Math.Min(w, src.width);
                        int copyH = Math.Min(h, src.height);
                        var colors = src.GetPixels(0, 0, copyW, copyH);
                        int dstX = Mathf.RoundToInt(pos.X);
                        int dstY = tex.height - Mathf.RoundToInt(pos.Y) - copyH;
                        tex.SetPixels(dstX, dstY, copyW, copyH, colors);
                    }
                ));
                break;
            default:
                return;
        }
        MarkDirty();
    }

    public IAbstTexture2D GetTexture(string? name = null)
    {
        Render();
        return new UnityTexture2D(_texture, name ?? $"UnityImage_{Width}x{Height}");
    }

    private APoint? EnsureCapacity(int minW, int minH)
    {
        int newW = Math.Max(Width, minW);
        int newH = Math.Max(Height, minH);
        if (newW == Width && newH == Height) return null;
        return new APoint(newW, newH);
    }
}

