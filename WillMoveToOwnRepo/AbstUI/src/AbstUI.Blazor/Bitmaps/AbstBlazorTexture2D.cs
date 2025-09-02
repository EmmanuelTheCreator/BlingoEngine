using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AbstUI.Blazor;
using AbstUI.Bitmaps;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Bitmaps;

/// <summary>
/// Texture backed by an off-screen HTML <canvas> element.
/// </summary>
public class AbstBlazorTexture2D : AbstBaseTexture2D<ElementReference>
{
    private readonly IJSRuntime _jsRuntime;
    public ElementReference Canvas { get; }
    public override int Width { get; }
    public override int Height { get; }

    protected AbstBlazorTexture2D(IJSRuntime jsRuntime, ElementReference canvas, int width, int height, string name = "") : base(name)
    {
        _jsRuntime = jsRuntime;
        Canvas = canvas;
        Width = width;
        Height = height;
    }

    public static async Task<AbstBlazorTexture2D> CreateAsync(IJSRuntime jsRuntime, int width, int height, string name = "")
    {
        var canvas = await jsRuntime.InvokeAsync<ElementReference>("abstCanvas.createCanvas", width, height);
        return new AbstBlazorTexture2D(jsRuntime, canvas, width, height, name);
    }

    public static async Task<AbstBlazorTexture2D> CreateFromPixelDataAsync(IJSRuntime jsRuntime, AbstUIScriptResolver scripts, byte[] data, int width, int height, string name = "")
    {
        var tex = await CreateAsync(jsRuntime, width, height, name);
        var ctx = await scripts.CanvasGetContext(tex.Canvas, false);
        await scripts.CanvasDrawPictureData(ctx, data, width, height, 0, 0);
        return tex;
    }

    protected override void DisposeTexture()
    {
        _ = _jsRuntime.InvokeVoidAsync("abstCanvas.disposeCanvas", Canvas);
    }

    public async Task<byte[]> GetPixelDataAsync(AbstUIScriptResolver scripts)
    {
        var ctx = await scripts.CanvasGetContext(Canvas, false);
        return await scripts.CanvasGetImageData(ctx, Width, Height);
    }

    public override byte[] GetPixels()
    {
        throw new NotImplementedException();
    }
}
