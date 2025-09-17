using AbstUI.SDL2.Components.Base;

namespace AbstUI.SDLTest.Fakes;

internal sealed record RenderSnapshot(
    string Name,
    float X,
    float Y,
    float OffsetX,
    float OffsetY,
    int TargetWidth,
    int TargetHeight,
    float Width,
    float Height)
{
    public float DrawX => X + OffsetX;
    public float DrawY => Y + OffsetY;

    public static RenderSnapshot FromComponent(AbstSdlComponent component)
    {
        var context = component.ComponentContext;
        return new RenderSnapshot(
            component.Name,
            context.X,
            context.Y,
            context.OffsetX,
            context.OffsetY,
            context.TargetWidth,
            context.TargetHeight,
            component.Width,
            component.Height);
    }
}
