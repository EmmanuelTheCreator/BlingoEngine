using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.SDL2.Primitives;

/// <summary>
/// Helper conversions for SDL pixel data masks.
/// </summary>
public static class LingoPixelFormatExtensions
{
    /// <summary>Gets color masks and bits per pixel for the given format.</summary>
    public static void GetMasks(this APixelFormat format, out uint rmask, out uint gmask, out uint bmask, out uint amask, out int bpp)
    {
        switch (format)
        {
            case APixelFormat.Rgba8888:
                rmask = 0x000000FF; gmask = 0x0000FF00; bmask = 0x00FF0000; amask = 0xFF000000; bpp = 32; break;
            case APixelFormat.Rgb888:
                rmask = 0x000000FF; gmask = 0x0000FF00; bmask = 0x00FF0000; amask = 0; bpp = 24; break;
            case APixelFormat.Rgb5650:
            case APixelFormat.Rgb5550:
                rmask = 0xF800; gmask = 0x07E0; bmask = 0x001F; amask = 0; bpp = 16; break;
            case APixelFormat.Rgba5551:
                rmask = 0x7C00; gmask = 0x03E0; bmask = 0x001F; amask = 0x8000; bpp = 16; break;
            case APixelFormat.Rgba4444:
                rmask = 0x0F00; gmask = 0x00F0; bmask = 0x000F; amask = 0xF000; bpp = 16; break;
            default:
                rmask = 0x000000FF; gmask = 0x0000FF00; bmask = 0x00FF0000; amask = 0; bpp = 24; break;
        }
    }
}
