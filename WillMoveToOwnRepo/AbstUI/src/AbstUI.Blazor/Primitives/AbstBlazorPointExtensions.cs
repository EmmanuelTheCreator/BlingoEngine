using System.Drawing;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Primitives;

/// <summary>
/// Helpers for translating between <see cref="APoint"/>/<see cref="ARect"/> and System.Drawing types.
/// </summary>
public static class AbstBlazorPointExtensions
{
    public static PointF ToPointF(this APoint point)
        => new(point.X, point.Y);

    public static APoint ToAbstPoint(this PointF point)
        => new(point.X, point.Y);

    public static RectangleF ToRectangleF(this ARect rect)
        => new(rect.Left, rect.Top, rect.Width, rect.Height);

    public static ARect ToAbstRect(this RectangleF rect)
        => ARect.New(rect.Left, rect.Top, rect.Width, rect.Height);
}
