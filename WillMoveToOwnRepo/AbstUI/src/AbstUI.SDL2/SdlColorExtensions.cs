using System.Numerics;
using AbstUI.Primitives;

namespace AbstUI.SDL2;

/// <summary>
/// Extension helpers for converting <see cref="AColor"/> values to SDL-friendly types.
/// </summary>
public static class SdlColorExtensions
{
    /// <summary>Converts a <see cref="AColor"/> to an ImGui-compatible <see cref="Vector4"/>.</summary>
    public static Vector4 ToImGuiColor(this AColor color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}
