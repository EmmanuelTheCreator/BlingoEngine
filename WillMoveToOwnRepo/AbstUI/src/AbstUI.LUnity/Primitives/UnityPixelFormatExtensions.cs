using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Primitives;

/// <summary>
/// Conversion helpers between <see cref="APixelFormat"/> and Unity's <see cref="TextureFormat"/>.
/// </summary>
public static class UnityPixelFormatExtensions
{
    /// <summary>Converts an <see cref="APixelFormat"/> to a Unity <see cref="TextureFormat"/>.</summary>
    public static TextureFormat ToUnityFormat(this APixelFormat format) => format switch
    {
        APixelFormat.Rgba8888 => TextureFormat.RGBA32,
        APixelFormat.Rgb888 => TextureFormat.RGB24,
        APixelFormat.Rgb5650 or APixelFormat.Rgb5550 => TextureFormat.RGB565,
        APixelFormat.Rgba5551 => TextureFormat.ARGB4444,
        APixelFormat.Rgba4444 => TextureFormat.RGBA4444,
        _ => TextureFormat.RGBA32,
    };

    /// <summary>Converts a Unity <see cref="TextureFormat"/> to an <see cref="APixelFormat"/>.</summary>
    public static APixelFormat ToAbstFormat(this TextureFormat format) => format switch
    {
        TextureFormat.RGBA32 => APixelFormat.Rgba8888,
        TextureFormat.RGB24 => APixelFormat.Rgb888,
        TextureFormat.RGB565 => APixelFormat.Rgb5650,
        TextureFormat.ARGB4444 => APixelFormat.Rgba5551,
        TextureFormat.RGBA4444 => APixelFormat.Rgba4444,
        _ => APixelFormat.Rgb888,
    };
}
