using System.Numerics;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Primitives;

public static class AbstColorExtensions
{
    public static Vector4 ToImGuiColor(this AColor color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public static string ToCss(this AColor color)
        => $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255f:F2})";
}
