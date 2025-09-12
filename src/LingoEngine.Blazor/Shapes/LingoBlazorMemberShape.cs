using System;
using System.Linq;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Blazor.Primitives;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Blazor.Util;
using LingoEngine.Primitives;
using LingoEngine.Shapes;
using LingoEngine.Sprites;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace LingoEngine.Blazor.Shapes;

/// <summary>
/// Basic Blazor implementation of a vector shape member. It draws the shape
/// onto an off-screen canvas using JavaScript interop and exposes it as a
/// texture when requested.
/// </summary>
public class LingoBlazorMemberShape : ILingoFrameworkMemberShape, IDisposable
{
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private AbstBlazorTexture2D? _texture;
    private byte[]? _pixelData;
    private int _stride;
    private LingoMemberShape _member = null!;

    public LingoList<APoint> VertexList { get; } = new();
    public LingoShapeType ShapeType { get; set; } = LingoShapeType.Rectangle;
    public AColor FillColor { get; set; } = AColor.FromRGB(255, 255, 255);
    public AColor EndColor { get; set; } = AColor.FromRGB(255, 255, 255);
    public AColor StrokeColor { get; set; } = AColor.FromRGB(0, 0, 0);
    public int StrokeWidth { get; set; } = 1;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool Closed { get; set; } = true;
    public bool AntiAlias { get; set; } = true;
    public bool Filled { get; set; } = true;
    public bool IsLoaded { get; private set; }
    public IAbstTexture2D? TextureLingo => _texture;

    public LingoBlazorMemberShape(IJSRuntime js, AbstUIScriptResolver scripts)
    {
        _js = js;
        _scripts = scripts;
    }

    internal void Init(LingoMemberShape member) => _member = member;

    public void Preload()
    {
        if (IsLoaded)
            return;
        int w = Math.Max(1, (int)Width);
        int h = Math.Max(1, (int)Height);
        _texture = AbstBlazorTexture2D.CreateAsync(_js, w, h).GetAwaiter().GetResult();
        var ctx = _scripts.CanvasGetContext(_texture.Canvas, !AntiAlias).GetAwaiter().GetResult();
        string fill = FillColor.ToCss();
        string stroke = StrokeColor.ToCss();
        switch (ShapeType)
        {
            case LingoShapeType.Rectangle:
                _scripts.CanvasDrawRect(ctx, 0, 0, w, h, Filled ? fill : stroke, Filled, StrokeWidth)
                        .GetAwaiter().GetResult();
                break;
            case LingoShapeType.Oval:
                int radius = Math.Min(w, h) / 2;
                _scripts.CanvasDrawCircle(ctx, w / 2, h / 2, radius, Filled ? fill : stroke, Filled, StrokeWidth)
                        .GetAwaiter().GetResult();
                break;
            case LingoShapeType.Line:
                if (VertexList.Count >= 2)
                    _scripts.CanvasDrawLine(ctx,
                        VertexList[0].X, VertexList[0].Y,
                        VertexList[1].X, VertexList[1].Y, stroke, StrokeWidth)
                        .GetAwaiter().GetResult();
                break;
            case LingoShapeType.PolyLine:
                if (VertexList.Count >= 2)
                {
                    var pts = VertexList.SelectMany(p => new double[] { p.X, p.Y }).ToArray();
                    _scripts.CanvasDrawPolygon(ctx, pts, stroke, Filled, StrokeWidth)
                            .GetAwaiter().GetResult();
                }
                break;
            case LingoShapeType.RoundRect:
                _scripts.CanvasDrawRect(ctx, 0, 0, w, h, Filled ? fill : stroke, Filled, StrokeWidth)
                        .GetAwaiter().GetResult();
                break;
        }
        _pixelData = _texture.GetPixelDataAsync(_scripts).GetAwaiter().GetResult();
        _stride = w * 4;
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }

    public void Unload()
    {
        _texture?.Dispose();
        _texture = null;
        IsLoaded = false;
    }

    public void Erase()
    {
        Unload();
        VertexList.Clear();
    }

    public void CopyToClipboard() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public bool IsPixelTransparent(int x, int y)
        => PixelDataUtils.IsTransparent(_pixelData, _stride, (int)Width, (int)Height, x, y);

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => _texture;

    public void Dispose() => Unload();
}

