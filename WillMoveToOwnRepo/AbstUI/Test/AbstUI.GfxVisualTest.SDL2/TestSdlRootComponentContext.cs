using System;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;

namespace LingoEngine.SDL2.GfxVisualTest;

public sealed class TestSdlRootComponentContext : AbstUISdlRootContext<AbstMouse>
{
    private readonly SdlMouse<AbstMouseEvent> _sdlMouse;
    private readonly SdlKey _sdlKey;

    public TestSdlRootComponentContext()
        : base(CreateWindowAndRenderer(out var renderer), renderer, new SdlFocusManager())
    {
        _sdlMouse = new SdlMouse<AbstMouseEvent>(new Lazy<AbstMouse<AbstMouseEvent>>(() => AbstMouse));
        AbstMouse = new AbstMouse(_sdlMouse);

        _sdlKey = new SdlKey();
        AbstKey = new AbstKey(_sdlKey);
        _sdlKey.SetKeyObj((AbstKey)AbstKey);

        GlobalMouse = new GlobalSDLAbstMouse();
        GlobalKey = new GlobalSDLAbstKey();
    }

    private static nint CreateWindowAndRenderer(out nint renderer)
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG | SDL_image.IMG_InitFlags.IMG_INIT_JPG);
        SDL_ttf.TTF_Init();
        SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_MP3);

        var window = SDL.SDL_CreateWindow(
            "AbstUI SDL2 Visual Test",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            800,
            600,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        renderer = SDL.SDL_CreateRenderer(
            window,
            -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        return window;
    }

    protected override void Render()
    {
        //SDL.SDL_SetRenderDrawColor(Renderer, 200, 200, 150, 255);
        //SDL.SDL_RenderClear(Renderer);
        base.Render();
    }

    public override void Dispose()
    {
        SDL_ttf.TTF_Quit();
        SDL_mixer.Mix_Quit();
        base.Dispose();
    }
}
