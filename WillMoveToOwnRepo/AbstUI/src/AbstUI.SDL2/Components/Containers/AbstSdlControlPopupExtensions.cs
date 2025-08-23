using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Containers;

internal static class AbstSdlControlPopupExtensions
{
    public static (int x, int y) GetScreenPosition(this AbstSDLComponentContext ctx)
    {
        int x = 0, y = 0;
        while (ctx != null)
        {
            x += ctx.X + (int)ctx.OffsetX;
            y += ctx.Y + (int)ctx.OffsetY;
            if (ctx.Component is AbstSdlScrollViewer sv)
            {
                x -= (int)sv.ScrollHorizontal;
                y -= (int)sv.ScrollVertical;
            }
            ctx = ctx.Parent;
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

