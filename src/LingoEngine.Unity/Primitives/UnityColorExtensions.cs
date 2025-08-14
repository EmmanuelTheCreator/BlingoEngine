using LingoEngine.Primitives;
using UnityEngine;

namespace LingoEngine.Unity.Primitives;

/// <summary>
/// Conversion helpers between <see cref="LingoColor"/> and Unity's <see cref="Color"/>.
/// </summary>
public static class UnityColorExtensions
{
    /// <summary>Converts a Unity <see cref="Color"/> to a <see cref="LingoColor"/>.</summary>
    public static LingoColor ToLingoColor(this Color color)
        => LingoColor.FromRGB(
            (byte)(color.r * 255),
            (byte)(color.g * 255),
            (byte)(color.b * 255),
            -1,
            string.Empty,
            (byte)(color.a * 255));

    /// <summary>Converts a <see cref="LingoColor"/> to a Unity <see cref="Color"/>.</summary>
    public static Color ToUnityColor(this LingoColor color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}
