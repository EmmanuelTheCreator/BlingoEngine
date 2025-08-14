using LingoEngine.Bitmaps;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Pictures;

public class SdlImageTexture : ILingoTexture2D
{
    private SDL.SDL_Surface _surfacePtr;
    public SDL.SDL_Surface Ptr => _surfacePtr;

    public nint SurfaceId { get; }
    public int Width { get; set; }

    public int Height { get; set; }

    public SdlImageTexture(SDL.SDL_Surface surfacePtr, nint surfaceId, int width, int height)
    {
        _surfacePtr = surfacePtr;
        SurfaceId = surfaceId;
        Width = width;
        Height = height;
    }
}

public class SdlTexture2D : ILingoTexture2D
{
    public nint Texture { get; }
    public int Width { get; }
    public int Height { get; }

    public SdlTexture2D(nint texture, int width, int height)
    {
        Texture = texture;
        Width = width;
        Height = height;
    }
}
