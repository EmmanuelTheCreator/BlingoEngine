using Godot;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.LGodot.Primitives;

/// <summary>
/// Extensions for converting <see cref="APixelFormat"/> to and from Godot formats.
/// </summary>
public static class LingoPixelFormatExtensions
{
    public static Image.Format ToGodotFormat(this APixelFormat format) => format switch
    {
        APixelFormat.Rgba8888 => Image.Format.Rgba8,
        APixelFormat.Rgb888 => Image.Format.Rgb8,
        APixelFormat.Rgb5650 => Image.Format.Rgb565,
        APixelFormat.Rgb5550 => Image.Format.Rgb565,
        APixelFormat.Rgba5551 => Image.Format.Rgba4444,
        APixelFormat.Rgba4444 => Image.Format.Rgba4444,
        _ => Image.Format.Rgb8,
    };

    public static APixelFormat ToLingoFormat(this Image.Format format) => format switch
    {
        Image.Format.Rgba8 => APixelFormat.Rgba8888,
        Image.Format.Rgb8 => APixelFormat.Rgb888,
        Image.Format.Rgb565 => APixelFormat.Rgb5650,
        Image.Format.Rgba4444 => APixelFormat.Rgba4444,
        _ => APixelFormat.Rgb888,
    };
}
