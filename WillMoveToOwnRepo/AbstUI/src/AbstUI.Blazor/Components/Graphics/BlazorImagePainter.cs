using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using AbstUI.Blazor.Bitmaps;

namespace AbstUI.Blazor.Components.Graphics;

/// <summary>
/// Off-screen canvas painter for the Blazor backend.
/// Accumulates draw operations and renders them to an HTML canvas via JS interop.
/// </summary>
public class BlazorImagePainter : IAbstImagePainter
{
    private readonly IAbstFontManager _fontManager;
    private readonly IJSRuntime _jsRuntime;
    private readonly AbstUIScriptResolver _scripts;
    private AbstBlazorTexture2D _texture;
    private IJSObjectReference? _context;
    private readonly List<(Func<APoint?> GetTotalSize, Func<IJSObjectReference, Task> DrawAction)> _drawActions = new();
    private AColor? _clearColor;
    private bool _dirty;
    private bool _pixilated;

    public int MaxWidth { get; set; } = 16384;
    public int MaxHeight { get; set; } = 16384;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Pixilated
    {
        get => _pixilated;
        set
        {
            if (_pixilated != value)
            {
                _pixilated = value;
                if (_texture != null)
                    _context = _scripts.CanvasGetContext(_texture.Canvas, _pixilated).GetAwaiter().GetResult();
            }
        }
    }
    public bool AutoResize { get; set; } = true;

    public BlazorImagePainter(IAbstFontManager fontManager, IJSRuntime jsRuntime, AbstUIScriptResolver scripts, int width = 0, int height = 0)
    {
        _fontManager = fontManager;
        _jsRuntime = jsRuntime;
        _scripts = scripts;
        Width = width > 0 ? Math.Min(width, MaxWidth) : 10;
        Height = height > 0 ? Math.Min(height, MaxHeight) : 10;
        _texture = AbstBlazorTexture2D.CreateAsync(jsRuntime, Width, Height, $"BlazorImage_{Width}x{Height}").GetAwaiter().GetResult();
        _context = _scripts.CanvasGetContext(_texture.Canvas, Pixilated).GetAwaiter().GetResult();
        _dirty = true;
    }

    public void Dispose()
    {
        _texture.Dispose();
    }

    private void MarkDirty() => _dirty = true;

    public void Render()
    {
        if (!_dirty)
            return;

        var newWidth = Width;
        var newHeight = Height;
        if (AutoResize)
        {
            foreach (var a in _drawActions)
            {
                var ns = a.GetTotalSize();
                if (ns != null && (ns.Value.X > newWidth || ns.Value.Y > newHeight))
                {
                    newWidth = (int)MathF.Max(newWidth, ns.Value.X);
                    newHeight = (int)MathF.Max(newHeight, ns.Value.Y);
                }
            }
        }

        newWidth = Math.Min(newWidth, MaxWidth);
        newHeight = Math.Min(newHeight, MaxHeight);
        if (newWidth > Width || newHeight > Height)
        {
            _texture.Dispose();
            _texture = AbstBlazorTexture2D.CreateAsync(_jsRuntime, newWidth, newHeight, $"BlazorImage_{newWidth}x{newHeight}").GetAwaiter().GetResult();
            _context = _scripts.CanvasGetContext(_texture.Canvas, Pixilated).GetAwaiter().GetResult();
            Width = newWidth;
            Height = newHeight;
        }

        var ctx = _context!;
        var clear = _clearColor ?? AColor.FromRGBA(0, 0, 0, 0);
        _scripts.CanvasClear(ctx, ToCss(clear), Width, Height).GetAwaiter().GetResult();
        foreach (var action in _drawActions)
            action.DrawAction(ctx).GetAwaiter().GetResult();
        _dirty = false;
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    public void SetPixel(APoint point, AColor color)
    {
        var p = point; var c = color;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)p.X + 1, (int)p.Y + 1) : null,
            ctx => _scripts.CanvasSetPixel(ctx, (int)p.X, (int)p.Y, ToCss(c))
        ));
        MarkDirty();
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start; var e = end; var c = color; var w = (int)MathF.Max(1, width);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
                int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
                return EnsureCapacity(maxX, maxY);
            },
            ctx => _scripts.CanvasDrawLine(ctx, s.X, s.Y, e.X, e.Y, ToCss(c), w)
        ));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect; var c = color; var w = (int)MathF.Max(1, width); var f = filled;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)r.Right, (int)r.Bottom) : null,
            ctx => _scripts.CanvasDrawRect(ctx, r.Left, r.Top, r.Width, r.Height, ToCss(c), f, w)
        ));
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var ctr = center; var rad = radius; var c = color; var f = filled; var w = (int)MathF.Max(1, width);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int maxX = (int)MathF.Ceiling(ctr.X + rad) + 1;
                int maxY = (int)MathF.Ceiling(ctr.Y + rad) + 1;
                return EnsureCapacity(maxX, maxY);
            },
            ctx => _scripts.CanvasDrawCircle(ctx, ctr.X, ctr.Y, rad, ToCss(c), f, w)
        ));
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var ctr = center; var rad = radius; var sd = startDeg; var ed = endDeg; var c = color; var w = (int)MathF.Max(1, width);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int maxX = (int)MathF.Ceiling(ctr.X + rad) + 1;
                int maxY = (int)MathF.Ceiling(ctr.Y + rad) + 1;
                return EnsureCapacity(maxX, maxY);
            },
            ctx => _scripts.CanvasDrawArc(ctx, ctr.X, ctr.Y, rad, sd, ed, ToCss(c), w)
        ));
        MarkDirty();
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        if (points.Count == 0) return;
        var pts = points; var c = color; var f = filled; var w = (int)MathF.Max(1, width);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                float maxX = 0; float maxY = 0;
                foreach (var p in pts)
                {
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }
                return EnsureCapacity((int)MathF.Ceiling(maxX) + 1, (int)MathF.Ceiling(maxY) + 1);
            },
            ctx =>
            {
                var flat = new double[pts.Count * 2];
                for (int i = 0; i < pts.Count; i++)
                {
                    flat[i * 2] = pts[i].X;
                    flat[i * 2 + 1] = pts[i].Y;
                }
                return _scripts.CanvasDrawPolygon(ctx, flat, ToCss(c), f, w);
            }
        ));
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position; var txt = text; var fnt = font; var col = color ?? AColors.Black; var fs = fontSize; var w = width; var align = alignment;
        var fi = _fontManager.GetFontInfo(fnt ?? string.Empty, fs);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                float textW = w >= 0 ? w : _fontManager.MeasureTextWidth(txt, fnt ?? string.Empty, fs);
                return EnsureCapacity((int)(pos.X + textW), (int)(pos.Y + fi.FontHeight - fi.TopIndentation));
            },
            ctx =>
            {
                var alignStr = align switch
                {
                    AbstTextAlignment.Center => "center",
                    AbstTextAlignment.Right => "right",
                    _ => "left"
                };
                return _scripts.CanvasDrawText(ctx, pos.X, pos.Y - fi.TopIndentation, txt, fnt ?? string.Empty, ToCss(col), fs, alignStr);
            }
        ));
        MarkDirty();
    }

    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position; var txt = text; var fnt = font; var col = color ?? AColors.Black; var fs = fontSize; var w = width; var h = height; var align = alignment;
        var fi = _fontManager.GetFontInfo(fnt ?? string.Empty, fs);
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int needW = w >= 0 ? w : (int)_fontManager.MeasureTextWidth(txt, fnt ?? string.Empty, fs);
                int needH = h >= 0 ? h : fi.FontHeight - fi.TopIndentation;
                return EnsureCapacity((int)(pos.X + needW), (int)(pos.Y + needH));
            },
            ctx =>
            {
                var alignStr = align switch
                {
                    AbstTextAlignment.Center => "center",
                    AbstTextAlignment.Right => "right",
                    _ => "left"
                };
                return _scripts.CanvasDrawText(ctx, pos.X, pos.Y - fi.TopIndentation, txt, fnt ?? string.Empty, ToCss(col), fs, alignStr);
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        var dat = data; var w = width; var h = height; var pos = position;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)pos.X + w, (int)pos.Y + h) : null,
            ctx => _scripts.CanvasDrawPictureData(ctx, dat, w, h, (int)pos.X, (int)pos.Y)
        ));
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        var tex = texture; var w = width; var h = height; var pos = position;
        _drawActions.Add((
            () => AutoResize ? EnsureCapacity((int)pos.X + w, (int)pos.Y + h) : null,
            async ctx =>
            {
                if (tex is AbstBlazorTexture2D btex)
                {
                    var data = await btex.GetPixelDataAsync(_scripts);
                    await _scripts.CanvasDrawPictureData(ctx, data, w, h, (int)pos.X, (int)pos.Y);
                }
            }
        ));
        MarkDirty();
    }

    public IAbstTexture2D GetTexture(string? name = null)
    {
        Render();
        _texture.Name = name ?? _texture.Name;
        return _texture;
    }

    private static string ToCss(AColor c) => $"rgba({c.R},{c.G},{c.B},{c.A / 255f})";

    private APoint? EnsureCapacity(int minW, int minH)
    {
        int newW = Math.Max(Width, minW);
        int newH = Math.Max(Height, minH);
        if (newW == Width && newH == Height) return null;
        return new APoint(newW, newH);
    }
}
