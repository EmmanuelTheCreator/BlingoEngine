using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Bitmaps;

/// <summary>
/// Texture created from an existing DOM image element.
/// </summary>
public class BlazorImageTexture : AbstBlazorTexture2D
{
    public BlazorImageTexture(IJSRuntime jsRuntime, ElementReference element, int width, int height)
        : base(jsRuntime, element, width, height)
    {
    }
}
