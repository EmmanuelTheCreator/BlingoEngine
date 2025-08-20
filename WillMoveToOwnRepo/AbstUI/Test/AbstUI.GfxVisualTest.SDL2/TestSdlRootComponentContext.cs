using System;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;

namespace LingoEngine.SDL2.GfxVisualTest;

/// <summary>
/// Minimal SDL2 environment for the AbstUI graphics visual test.
/// Provides the <see cref="ISdlRootComponentContext"/> required by
/// <see cref="AbstSdlComponentFactory"/> and runs a basic render loop.
/// </summary>
public sealed class TestSdlRootComponentContext : ISdlRootComponentContext, IDisposable
{
    private readonly SdlMouse<AbstMouseEvent> _sdlMouse;
    private readonly SdlKey _sdlKey;
    private readonly AbstMouse _abstMouse;
    private readonly AbstKey _abstKey;

    public nint Window { get; }
    public nint Renderer { get; }

    public AbstSDLComponentContainer ComponentContainer { get; }
    public IAbstMouse AbstMouse => _abstMouse;
    public IAbstKey AbstKey => _abstKey;
    public SdlFocusManager FocusManager { get; }

    public TestSdlRootComponentContext()
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG | SDL_image.IMG_InitFlags.IMG_INIT_JPG);
        SDL_ttf.TTF_Init();
        SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_MP3); 

        Window = SDL.SDL_CreateWindow(
            "AbstUI SDL2 Visual Test",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            800,
            600,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        Renderer = SDL.SDL_CreateRenderer(
            Window,
            -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        FocusManager = new SdlFocusManager();
        ComponentContainer = new AbstSDLComponentContainer(FocusManager);

        _sdlMouse = new SdlMouse<AbstMouseEvent>(new Lazy<AbstMouse<AbstMouseEvent>>(() => _abstMouse));
        _abstMouse = new AbstMouse(_sdlMouse);

        _sdlKey = new SdlKey();
        _abstKey = new AbstKey(_sdlKey);
        _sdlKey.SetKeyObj(_abstKey);
    }

    /// <summary>
    /// Runs a simple SDL2 loop until a key is pressed or the window is closed.
    /// </summary>
    public void Run(AbstSdlComponentFactory factory)
    {
        bool running = true;
        while (running && !Console.KeyAvailable)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
                _sdlKey.ProcessEvent(e);
                _sdlMouse.ProcessEvent(e);
                ComponentContainer.HandleEvent(e);
            }

            SDL.SDL_SetRenderDrawColor(Renderer, 200, 200, 150, 255);
            SDL.SDL_RenderClear(Renderer);
            ComponentContainer.Render(factory.CreateRenderContext());
            SDL.SDL_RenderPresent(Renderer);

            SDL.SDL_Delay(16);
        }
    }

    public APoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(Window, out var w, out var h);
        return new APoint(w, h);
    }

    public void Dispose()
    {
        SDL.SDL_DestroyRenderer(Renderer);
        SDL.SDL_DestroyWindow(Window);
        SDL_ttf.TTF_Quit();
        SDL_image.IMG_Quit();
        SDL_mixer.Mix_Quit();
        SDL.SDL_Quit();
    }
}

