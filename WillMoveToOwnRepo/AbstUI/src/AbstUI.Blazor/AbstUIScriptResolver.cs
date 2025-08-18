using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor;

/// <summary>
/// Provides access to the JavaScript helper module used by AbstUI.
/// The resolver loads the underlying ES module on first use and
/// exposes strongly typed proxies for all exported functions so that
/// consumers do not have to deal with <see cref="IJSObjectReference"/>
/// directly.
/// </summary>
public class AbstUIScriptResolver : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private Task<IJSObjectReference>? _moduleTask;

    public AbstUIScriptResolver(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private Task<IJSObjectReference> GetModuleAsync()
        => _moduleTask ??= _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js").AsTask();

    public async ValueTask<ElementReference> CanvasCreateCanvas(int width, int height)
        => await (await GetModuleAsync()).InvokeAsync<ElementReference>("abstCanvas.createCanvas", width, height);

    public async ValueTask CanvasDisposeCanvas(ElementReference canvas)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.disposeCanvas", canvas);

    public async ValueTask<IJSObjectReference> CanvasGetContext(ElementReference canvas, bool pixilated)
        => await (await GetModuleAsync()).InvokeAsync<IJSObjectReference>("abstCanvas.getContext", canvas, pixilated);

    public async ValueTask CanvasClear(IJSObjectReference ctx, string color, int width, int height)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.clear", ctx, color, width, height);

    public async ValueTask CanvasSetPixel(IJSObjectReference ctx, int x, int y, string color)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.setPixel", ctx, x, y, color);

    public async ValueTask CanvasDrawLine(IJSObjectReference ctx, double x1, double y1, double x2, double y2, string color, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawLine", ctx, x1, y1, x2, y2, color, width);

    public async ValueTask CanvasDrawRect(IJSObjectReference ctx, double x, double y, double w, double h, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawRect", ctx, x, y, w, h, color, filled, width);

    public async ValueTask CanvasDrawCircle(IJSObjectReference ctx, double x, double y, double radius, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawCircle", ctx, x, y, radius, color, filled, width);

    public async ValueTask CanvasDrawArc(IJSObjectReference ctx, double x, double y, double radius, double startDeg, double endDeg, string color, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawArc", ctx, x, y, radius, startDeg, endDeg, color, width);

    public async ValueTask CanvasDrawPolygon(IJSObjectReference ctx, double[] points, string color, bool filled, int width)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawPolygon", ctx, points, color, filled, width);

    public async ValueTask CanvasDrawText(IJSObjectReference ctx, double x, double y, string text, string font, string color, int fontSize, string alignment)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawText", ctx, x, y, text, font, color, fontSize, alignment);

    public async ValueTask CanvasDrawPictureData(IJSObjectReference ctx, byte[] data, int width, int height, int x, int y)
        => await (await GetModuleAsync()).InvokeVoidAsync("abstCanvas.drawPictureData", ctx, data, width, height, x, y);

    public async ValueTask<byte[]> CanvasGetImageData(IJSObjectReference ctx, int width, int height)
        => await (await GetModuleAsync()).InvokeAsync<byte[]>("abstCanvas.getImageData", ctx, width, height);

    public async ValueTask SetCursor(string cursor)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIKey.setCursor", cursor);

    public async ValueTask ShowBootstrapModal(string id)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIWindow.showBootstrapModal", id);

    public async ValueTask HideBootstrapModal(string id)
        => await (await GetModuleAsync()).InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", id);

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is not null && _moduleTask.IsCompletedSuccessfully)
        {
            var module = await _moduleTask;
            await module.DisposeAsync();
        }
    }
}

