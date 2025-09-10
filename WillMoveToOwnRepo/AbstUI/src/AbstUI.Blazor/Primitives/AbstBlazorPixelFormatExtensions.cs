using System.Drawing.Imaging;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Primitives;

/// <summary>
/// Extensions for converting <see cref="APixelFormat"/> to and from <see cref="PixelFormat"/>.
/// </summary>
public static class AbstBlazorPixelFormatExtensions
{
    /// <summary>
    /// Converts an <see cref="APixelFormat"/> to a <see cref="PixelFormat"/> understood by <c>System.Drawing</c>.
    /// </summary>
    public static PixelFormat ToDrawingFormat(this APixelFormat format) => format switch
    {
        APixelFormat.Rgba8888 => PixelFormat.Format32bppArgb,
        APixelFormat.Rgb888 => PixelFormat.Format24bppRgb,
        APixelFormat.Rgb5650 => PixelFormat.Format16bppRgb565,
        APixelFormat.Rgb5550 => PixelFormat.Format16bppRgb555,
        APixelFormat.Rgba5551 => PixelFormat.Format16bppArgb1555,
        // System.Drawing does not support a true 4-4-4-4 format; map to the closest 16bpp ARGB option.
        APixelFormat.Rgba4444 => PixelFormat.Format16bppArgb1555,
        _ => PixelFormat.Format24bppRgb,
    };

    /// <summary>
    /// Converts a <see cref="PixelFormat"/> to an <see cref="APixelFormat"/>.
    /// </summary>
    public static APixelFormat ToAbstFormat(this PixelFormat format) => format switch
    {
        PixelFormat.Format32bppArgb or PixelFormat.Format32bppPArgb => APixelFormat.Rgba8888,
        PixelFormat.Format24bppRgb => APixelFormat.Rgb888,
        PixelFormat.Format16bppRgb565 => APixelFormat.Rgb5650,
        PixelFormat.Format16bppRgb555 => APixelFormat.Rgb5550,
        PixelFormat.Format16bppArgb1555 => APixelFormat.Rgba5551,
        _ => APixelFormat.Rgb888,
    };
}
