using LingoEngine.FrameworkCommunication;
using LingoEngine.SDL2.Core;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

namespace LingoEngine.SDL2.Stages;

internal class SdlDebugOverlay : ILingoFrameworkDebugOverlay, ILingoSDLComponent
{
    private nint _font;
    private SDL.SDL_Color _white;
    public LingoSDLComponentContext ComponentContext { get; }

    public SdlDebugOverlay(LingoSdlFactory factory)
    {
        ComponentContext = factory.CreateContext(this);

        string fullFileName = GetFileName();
        _font = SDL_ttf.TTF_OpenFont(fullFileName, 12);
        if (_font == nint.Zero)
            throw new Exception($"TTF_OpenFont failed: {SDL.SDL_GetError()}");
        _white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
    }

    public void Begin() { }

    public void ShowDebugger()
    {
        ComponentContext.Visible = true;
    }

    public void HideDebugger()
    {
        ComponentContext.Visible = false;
    }

    public int PrepareLine(int id, string text)
    {
        return id;
    }


    public void SetLineText(int id, string text)
    {
        nint surface = SDL_ttf.TTF_RenderUTF8_Blended(_font, text, _white);
        if (surface == nint.Zero)
            throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");

        nint texture = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, surface);
        if (texture == nint.Zero)
            throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

        SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);
        SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = 20, y = id * 15, w = w, h = h };

        SDL.SDL_RenderCopy(ComponentContext.Renderer, texture, nint.Zero, ref dstRect);

        SDL.SDL_DestroyTexture(texture);
        SDL.SDL_FreeSurface(surface);
        ComponentContext.QueueRedraw(this);
    }


    private static string GetFileName() => Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar + "Tahoma.ttf";


    public void End() { }

    public LingoSDLRenderResult Render(LingoSDLRenderContext context)
    {
        // renderer can change, so update if needed
        ComponentContext.Renderer = context.Renderer;
        return nint.Zero;
    }
    public void Dispose()
    {
        if (_font != nint.Zero)
        {
            SDL_ttf.TTF_CloseFont(_font);
            _font = nint.Zero;
        }
        ComponentContext.Dispose();
    }


}
