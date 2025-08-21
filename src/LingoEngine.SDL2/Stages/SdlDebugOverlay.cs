using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using LingoEngine.FrameworkCommunication;
using LingoEngine.SDL2.Core;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.SDL2.Stages;

internal class SdlDebugOverlay : ILingoFrameworkDebugOverlay, IAbstSDLComponent
{
    private nint _font;
    private SDL.SDL_Color _white;
    private nint _texture;
    private List<(int id, string text)> _lines = new();
    private bool _hasChanged;

    public AbstSDLComponentContext ComponentContext { get; }

    public SdlDebugOverlay(LingoSdlFactory factory)
    {
        ComponentContext = factory.CreateContext(this);
        string fullFileName = GetFileName();
        _font = SDL_ttf.TTF_OpenFont(fullFileName, 12);
        if (_font == nint.Zero)
            throw new Exception($"TTF_OpenFont failed: {SDL.SDL_GetError()}");
        _white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
        for (int i = 0; i < 6; i++)
            _lines.Add((0, ""));
    }

    public void Begin() {
        
    }

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

  
  


    private static string GetFileName() => Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar + "Tahoma.ttf";


    public void End() { }
    public void SetLineText(int id, string text)
    {
        if (_lines[id].text == text) return;
        _hasChanged = true;
        _lines[id] = (id, text);
        this.ComponentContext.QueueRedraw(this);
        //if (_texture != nint.Zero)
        //    SDL.SDL_DestroyTexture(_texture);

        //nint surface = SDL_ttf.TTF_RenderUTF8_Blended(_font, text, _white);
        //if (surface == nint.Zero)
        //    throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");

        //_texture = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, surface);
        //if (_texture == nint.Zero)
        //    throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

        //SDL.SDL_QueryTexture(_texture, out _, out _, out int w, out int h);
        //SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = 20, y = id * 15, w = w, h = h };

        //SDL.SDL_RenderCopy(ComponentContext.Renderer, _texture, nint.Zero, ref dstRect);


        //SDL.SDL_FreeSurface(surface);
        //SDL.SDL_DestroyTexture(_texture);
        //ComponentContext.QueueRedraw(this);
    }
    public AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (_hasChanged)
        {
            _hasChanged = false;
            // renderer can change, so update if needed
            ComponentContext.Renderer = context.Renderer;
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            var finalText = string.Join('\n', _lines.Select(line => line.text));

            if (finalText == "") return _texture;

            nint surface = SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(_font, finalText, _white,500);
            if (surface == nint.Zero)
                throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");

            _texture = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, surface);
            if (_texture == nint.Zero)
                throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

            SDL.SDL_QueryTexture(_texture, out _, out _, out int w, out int h);
            SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = 20, y = 20, w = w, h = h };

            SDL.SDL_RenderCopy(ComponentContext.Renderer, _texture, nint.Zero, ref dstRect);
            SDL.SDL_FreeSurface(surface);
        }
        return _texture;
    }
    public void Dispose()
    {
        if (_texture != nint.Zero)
            SDL.SDL_DestroyTexture(_texture);
        if (_font != nint.Zero)
        {
            SDL_ttf.TTF_CloseFont(_font);
            _font = nint.Zero;
        }
        ComponentContext.Dispose();
    }


}
