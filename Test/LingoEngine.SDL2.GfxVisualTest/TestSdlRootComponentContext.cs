using LingoEngine.Primitives;
using LingoEngine.SDL2;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.GfxVisualTest;

public class TestSdlRootComponentContext : ISdlRootComponentContext, IDisposable
{
    private readonly nint _window;
    public LingoSDLComponentContainer ComponentContainer { get; } = new();
    public nint Renderer { get; }

    public TestSdlRootComponentContext()
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_AUDIO);
        SDL_ttf.TTF_Init();
        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);

        _window = SDL.SDL_CreateWindow("SDL Gfx Visual Test", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 800, 600,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        Renderer = SDL.SDL_CreateRenderer(_window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        SDL.SDL_RenderSetLogicalSize(Renderer, 800, 600);
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
            }

            SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(Renderer);

            ComponentContainer.Render(new LingoSDLRenderContext(Renderer));

            SDL.SDL_RenderPresent(Renderer);
        }
    }

    public void Dispose()
    {
        SDL.SDL_DestroyRenderer(Renderer);
        SDL.SDL_DestroyWindow(_window);
        SDL_ttf.TTF_Quit();
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }

    public LingoPoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(_window, out var w, out var h);
        return new LingoPoint(w, h);
    }
}
