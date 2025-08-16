
namespace AbstUI.Blazor.Bitmaps;

public class BlazorImageTexture : BlazorTexture2D
{
    private Blazor.Blazor_Surface _surfacePtr;
    public Blazor.Blazor_Surface Ptr => _surfacePtr;
    public nint SurfaceId { get; private set; }



    public BlazorImageTexture(Blazor.Blazor_Surface surfacePtr, nint surfaceId, int width, int height)
        : base(nint.Zero, width, height) // Initialize with zero texture
    {
        _surfacePtr = surfacePtr;
        SurfaceId = surfaceId;
    }

}

