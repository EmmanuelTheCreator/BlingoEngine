using System.Numerics;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Primitives;

/// <summary>
/// Extension helpers for converting <see cref="LingoColor"/> values to SDL-friendly types.
/// </summary>
public static class SdlColorExtensions
{
    /// <summary>Converts a <see cref="LingoColor"/> to an ImGui-compatible <see cref="Vector4"/>.</summary>
    public static Vector4 ToImGuiColor(this LingoColor color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}
