using AbstUI.Components.Graphics;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AbstUI.Blazor.Components.Graphics;

public class AbstBlazorGfxCanvasComponent : AbstBlazorComponentModelBase, IAbstFrameworkGfxCanvas, IFrameworkFor<AbstGfxCanvas>
{
    private readonly IJSRuntime _js;
    private ElementReference _canvasRef;
    private IJSObjectReference? _module;
    private IJSObjectReference? _context;
    private readonly List<Func<IJSObjectReference, ValueTask>> _drawActions = new();
    private AColor? _clearColor;
    private bool _dirty;

    public bool Pixilated { get; set; }

    public AbstBlazorGfxCanvasComponent(IJSRuntime js)
    {
        _js = js;
    }

    internal void SetCanvas(ElementReference canvasRef) => _canvasRef = canvasRef;

    public async Task OnAfterRenderAsync()
    {
        if (_context == null)
        {
            _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js");
            _context = await _module.InvokeAsync<IJSObjectReference>("abstCanvas.getContext", _canvasRef, Pixilated);
        }

        if (_dirty && _context != null && _module != null)
        {
            if (_clearColor.HasValue)
                await _module.InvokeVoidAsync("abstCanvas.clear", _context, ToCss(_clearColor.Value), Width, Height);
            foreach (var action in _drawActions)
                await action(_context);
            _dirty = false;
        }
    }

    private void MarkDirty()
    {
        if (!_dirty)
        {
            _dirty = true;
            RaiseChanged();
        }
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    public void SetPixel(APoint point, AColor color)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.setPixel", ctx, point.X, point.Y, ToCss(color)));
        MarkDirty();
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawLine", ctx, start.X, start.Y, end.X, end.Y, ToCss(color), width));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawRect", ctx, rect.Left, rect.Top, rect.Width, rect.Height, ToCss(color), filled, width));
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawCircle", ctx, center.X, center.Y, radius, ToCss(color), filled, width));
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawArc", ctx, center.X, center.Y, radius, startDeg, endDeg, ToCss(color), width));
        MarkDirty();
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        if (points.Count == 0)
            return;
        var flat = new float[points.Count * 2];
        for (int i = 0; i < points.Count; i++)
        {
            flat[i * 2] = points[i].X;
            flat[i * 2 + 1] = points[i].Y;
        }
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawPolygon", ctx, flat, ToCss(color), filled, width));
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default)
    {
        var col = ToCss(color ?? AColors.Black);
        var align = alignment switch
        {
            AbstTextAlignment.Center => "center",
            AbstTextAlignment.Right => "right",
            _ => "left"
        };
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawText", ctx, position.X, position.Y, text, font, col, fontSize, align));
        MarkDirty();
    }
    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
            int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left,
            AbstFontStyle style = AbstFontStyle.Regular)
    {
        //var col = ToCss(color ?? AColors.Black);
        //var align = alignment switch
        //{
        //    AbstTextAlignment.Center => "center",
        //    AbstTextAlignment.Right => "right",
        //    _ => "left"
        //};
        //_drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawText", ctx, position.X, position.Y, text, font, col, fontSize, align));
        //MarkDirty();
        throw new NotImplementedException();
    }
    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        _drawActions.Add(ctx => _module!.InvokeVoidAsync("abstCanvas.drawPictureData", ctx, data, width, height, position.X, position.Y));
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        // Drawing from texture not yet supported in Blazor canvas backend.
        MarkDirty();
    }

    public override void Dispose()
    {
        _drawActions.Clear();
        base.Dispose();
    }

    private static string ToCss(AColor c) => $"rgba({c.R},{c.G},{c.B},{c.A / 255f})";

    public IAbstTexture2D GetTexture(string? name = null)
    {
        throw new NotImplementedException();
    }
}
