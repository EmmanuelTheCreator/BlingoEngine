using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Windowing;

namespace AbstUI.SDL2.Components.Containers;

internal static class AbstSdlControlPopupExtensions
{
    public static (int x, int y) GetScreenPosition(this AbstSDLComponentContext ctx)
    {
        int x = 0, y = 0;
        var current = ctx;
        while (current != null)
        {
            x += current.X + (int)current.OffsetX;
            y += current.Y + (int)current.OffsetY;
            if (current.Component is AbstSdlScrollViewer sv)
            {
                x -= (int)sv.ScrollHorizontal;
                y -= (int)sv.ScrollVertical;
            }
            if (current.Component is AbstSdlWindow)
            {
                y += AbstSdlWindow.TitleBarHeight;
            }
            current = current.Parent;
        }
        return (x, y);
    }

    public static void PositionBelow(this AbstSdlComponent popup, AbstSDLComponentContext ownerContext, float offsetY)
    {
        var (sx, sy) = ownerContext.GetScreenPosition();
        popup.X = sx;
        popup.Y = sy + offsetY;
    }
}

