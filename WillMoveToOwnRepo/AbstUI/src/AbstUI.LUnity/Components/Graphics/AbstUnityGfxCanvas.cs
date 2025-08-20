using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.LUnity.Bitmaps;
using AbstUI.LUnity.Primitives;
using AbstUI.Texts;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Components.Graphics;
using AbstUI.LUnity.Components.Base;

namespace AbstUI.LUnity.Components.Graphics;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkGfxCanvas"/> that records paint actions
/// and executes them onto a <see cref="Texture2D"/>.
/// </summary>
internal class AbstUnityGfxCanvas : AbstUnityComponent, IAbstFrameworkGfxCanvas, IDisposable
{
    private readonly RawImage _image;
    private readonly Texture2D _texture;
    private readonly GfxCanvasBehaviour _behaviour;
    private readonly List<Action<Texture2D>> _drawActions = new();
    private Color? _clearColor;
    private bool _dirty = true;

    public AbstUnityGfxCanvas(int width, int height)
        : base(CreateGameObject(out var image, out var behaviour))
    {
        _image = image;
        _behaviour = behaviour;
        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        _image.texture = _texture;
        _behaviour.Init(this);
        Width = width;
        Height = height;
    }

    private static GameObject CreateGameObject(out RawImage image, out GfxCanvasBehaviour behaviour)
    {
        var go = new GameObject("GfxCanvas", typeof(RectTransform));
        image = go.AddComponent<RawImage>();
        behaviour = go.AddComponent<GfxCanvasBehaviour>();
        return go;
    }

    public bool Pixilated
    {
        get => _texture.filterMode == FilterMode.Point;
        set => _texture.filterMode = value ? FilterMode.Point : FilterMode.Bilinear;
    }

    private void MarkDirty() => _dirty = true;

    internal void Redraw()
    {
        if (!_dirty) return;

        if (_clearColor.HasValue)
        {
            var col = _clearColor.Value;
            var arr = new Color[_texture.width * _texture.height];
            for (int i = 0; i < arr.Length; i++) arr[i] = col;
            _texture.SetPixels(arr);
        }

        foreach (var act in _drawActions)
            act(_texture);

        _texture.Apply();
        _dirty = false;
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color.ToUnityColor();
        MarkDirty();
    }

    private static void SetPixel(Texture2D tex, int x, int y, Color col)
    {
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return;
        tex.SetPixel(x, tex.height - 1 - y, col);
    }

    public void SetPixel(APoint point, AColor color)
    {
        var p = point;
        var c = color.ToUnityColor();
        _drawActions.Add(tex => SetPixel(tex, (int)p.X, (int)p.Y, c));
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
        _drawActions.Add(tex => DrawLine(tex, (int)s.X, (int)s.Y, (int)e.X, (int)e.Y, col, w));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect; var col = color.ToUnityColor(); var w = width;
        _drawActions.Add(tex =>
        {
            int x0 = Mathf.RoundToInt(r.Left);
            int y0 = Mathf.RoundToInt(r.Top);
            int x1 = Mathf.RoundToInt(r.Right);
            int y1 = Mathf.RoundToInt(r.Bottom);
            if (filled)
            {
                int rw = x1 - x0;
                int rh = y1 - y0;
                var arr = new Color[rw * rh];
                for (int i = 0; i < arr.Length; i++) arr[i] = col;
                tex.SetPixels(x0, tex.height - y1, rw, rh, arr);
            }
            else
            {
                DrawLine(tex, x0, y0, x1, y0, col, w);
                DrawLine(tex, x1, y0, x1, y1, col, w);
                DrawLine(tex, x1, y1, x0, y1, col, w);
                DrawLine(tex, x0, y1, x0, y0, col, w);
            }
        });
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var c = center; var rad = Mathf.RoundToInt(radius); var col = color.ToUnityColor(); var w = width;
        _drawActions.Add(tex =>
        {
            int cx = Mathf.RoundToInt(c.X);
            int cy = Mathf.RoundToInt(c.Y);
            for (int y = -rad; y <= rad; y++)
            {
                int xx = (int)Mathf.Sqrt(rad * rad - y * y);
                if (filled)
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
        });
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var c = center; var rad = radius; var s = startDeg; var e = endDeg; var segs = segments; var col = color.ToUnityColor(); var w = width;
        _drawActions.Add(tex =>
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
        });
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
        var col = color.ToUnityColor(); var w = width;
        _drawActions.Add(tex =>
        {
            for (int i = 0; i < pts.Length - 1; i++)
                DrawLine(tex, (int)pts[i].X, (int)pts[i].Y, (int)pts[i + 1].X, (int)pts[i + 1].Y, col, w);
            DrawLine(tex, (int)pts[^1].X, (int)pts[^1].Y, (int)pts[0].X, (int)pts[0].Y, col, w);
            if (filled)
            {
                var p0 = pts[0];
                for (int i = 1; i < pts.Length - 1; i++)
                    FillTriangle(tex, p0, pts[i], pts[i + 1], col);
            }
        });
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default)
    {
        var pos = position; var txt = text; var col = (color ?? new AColor(0, 0, 0)).ToUnityColor();
        var fnt = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var w = width;
        var align = alignment;
        _drawActions.Add(tex =>
        {
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            UnityEngine.Graphics.Blit(tex, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, tex.width, tex.height, 0);
            var style = new GUIStyle { font = fnt, fontSize = fontSize, normal = new GUIStyleState { textColor = col } };
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
        });
        MarkDirty();
    }

    private static TextureFormat ToUnityFormat(APixelFormat format) => format switch
    {
        APixelFormat.Rgba8888 => TextureFormat.RGBA32,
        APixelFormat.Rgb888 => TextureFormat.RGB24,
        APixelFormat.Rgb5650 or APixelFormat.Rgb5550 => TextureFormat.RGB565,
        APixelFormat.Rgba5551 => TextureFormat.ARGB4444,
        APixelFormat.Rgba4444 => TextureFormat.RGBA4444,
        _ => TextureFormat.RGBA32
    };

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        var dat = data; var w = width; var h = height; var pos = position; var fmt = format;
        _drawActions.Add(tex =>
        {
            var t = new Texture2D(w, h, ToUnityFormat(fmt), false);
            t.LoadRawTextureData(dat);
            t.Apply();
            var colors = t.GetPixels(0, 0, w, h);
            int dstX = Mathf.RoundToInt(pos.X);
            int dstY = tex.height - Mathf.RoundToInt(pos.Y) - h;
            tex.SetPixels(dstX, dstY, w, h, colors);
            UnityEngine.Object.Destroy(t);
        });
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        if (texture is not UnityTexture2D ut || ut.Texture == null) return;
        var w = width; var h = height; var pos = position; var src = ut.Texture;
        _drawActions.Add(tex =>
        {
            var colors = src.GetPixels(0, 0, w, h);
            int dstX = Mathf.RoundToInt(pos.X);
            int dstY = tex.height - Mathf.RoundToInt(pos.Y) - h;
            tex.SetPixels(dstX, dstY, w, h, colors);
        });
        MarkDirty();
    }

    public new void Dispose()
    {
        _drawActions.Clear();
        UnityEngine.Object.Destroy(_texture);
        base.Dispose();
    }

    private class GfxCanvasBehaviour : MonoBehaviour
    {
        private AbstUnityGfxCanvas? _canvas;
        public void Init(AbstUnityGfxCanvas canvas) => _canvas = canvas;
        private void LateUpdate() => _canvas?.Redraw();
    }
}
