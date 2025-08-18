using System.Numerics;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2;

/// <summary>
/// Extension helpers for converting <see cref="AColor"/> values to SDL-friendly types.
/// </summary>
public static class SdlColorExtensions
{
    /// <summary>Converts a <see cref="AColor"/> to a normalized <see cref="Vector4"/>.</summary>
    public static Vector4 ToVector4(this AColor color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    /// <summary>Converts a <see cref="AColor"/> to an <see cref="SDL.SDL_Color"/>.</summary>
    public static SDL.SDL_Color ToSDLColor(this AColor color)
        => new() { r = color.R, g = color.G, b = color.B, a = color.A };
}
