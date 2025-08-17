using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Bitmaps;

/// <summary>
/// Texture created from an existing DOM image element.
/// </summary>
public class BlazorImageTexture : BlazorTexture2D
{
    public BlazorImageTexture(ElementReference element, int width, int height)
        : base(element, width, height)
    {
    }
}
