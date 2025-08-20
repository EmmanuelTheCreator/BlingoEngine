using AbstUI.SDL2.Styles;

namespace AbstUI.SDL2.Core;
public class AbstSDLRenderContext
{
    public nint Renderer { get; }
    public System.Numerics.Vector2 Origin { get; }
    public SdlFontManager SdlFontManager { get; }

    public AbstSDLRenderContext(nint renderer, System.Numerics.Vector2 origin, SdlFontManager sdlFontManager)
    {
        Renderer = renderer;
        Origin = origin;
        SdlFontManager = sdlFontManager;
    }

    public AbstSDLRenderContext CreateNew(System.Numerics.Vector2 childOrigin)
        => new(Renderer, childOrigin, SdlFontManager);
}
