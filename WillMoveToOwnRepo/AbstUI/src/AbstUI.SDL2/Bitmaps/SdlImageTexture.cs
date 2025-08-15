using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Bitmaps;

public class SdlImageTexture : SdlTexture2D
{
    private SDL.SDL_Surface _surfacePtr;
    public SDL.SDL_Surface Ptr => _surfacePtr;
    public nint SurfaceId { get; private set; }



    public SdlImageTexture(SDL.SDL_Surface surfacePtr, nint surfaceId, int width, int height)
        : base(nint.Zero, width, height) // Initialize with zero texture
    {
        _surfacePtr = surfacePtr;
        SurfaceId = surfaceId;
    }

}

