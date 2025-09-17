using System.Runtime.InteropServices;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.SDL2.Core;

namespace BlingoEngine.SDL2.Stages;

internal class SdlDebugOverlay : IBlingoFrameworkDebugOverlay, IAbstSDLComponent
{
    private const int Width = 200;
    private const int Height = 100;
    private nint _font;
    private SDL.SDL_Color _white;
    private nint _texture;
    private List<(int id, string text)> _lines = new();
    private bool _hasChanged;

    public AbstSDLComponentContext ComponentContext { get; }

    public SdlDebugOverlay(BlingoSdlFactory factory)
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

    public void Begin()
    {

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
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            nint background = SDL.SDL_CreateRGBSurfaceWithFormat(0, Width, Height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
            if (background == nint.Zero)
                throw new Exception($"SDL_CreateRGBSurfaceWithFormat failed: {SDL.SDL_GetError()}");

            SDL.SDL_Surface bgStruct = Marshal.PtrToStructure<SDL.SDL_Surface>(background);
            uint fill = SDL.SDL_MapRGBA(bgStruct.format, 0, 0, 0, 128);
            SDL.SDL_FillRect(background, IntPtr.Zero, fill);

            var finalText = string.Join('\n', _lines.Select(line => line.text));
            if (!string.IsNullOrEmpty(finalText))
            {
                nint textSurface = SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(_font, finalText, _white, Width);
                if (textSurface == nint.Zero)
                    throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");
                SDL.SDL_BlitSurface(textSurface, IntPtr.Zero, background, IntPtr.Zero);
                SDL.SDL_FreeSurface(textSurface);
            }

            _texture = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, background);
            if (_texture == nint.Zero)
                throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

            ComponentContext.TargetWidth = Width;
            ComponentContext.TargetHeight = Height;
            SDL.SDL_FreeSurface(background);
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

