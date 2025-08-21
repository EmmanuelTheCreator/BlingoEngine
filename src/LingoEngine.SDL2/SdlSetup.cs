using AbstUI.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.SDL2.Core;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2;
using LingoEngine.Core;
using AbstUI.SDL2.Core;
using AbstUI.Inputs;
using AbstUI;

namespace LingoEngine.SDL2;

public static class SdlSetup
{
    private static bool _engineRegistered = false;
    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, string windowTitle, int width, int height, Action<LingoSdlFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
    {
        if (_engineRegistered) return reg; // only register once
        _engineRegistered = true;
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.SDL2;
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_AUDIO) < 0)
        {
            Console.WriteLine("Unable to initialize SDL. Error: {0}", SDL.SDL_GetError());
            return reg;
        }
        if (SDL_ttf.TTF_Init() != 0)
        {
            Console.WriteLine($"TTF_Init failed: {SDL.SDL_GetError()}");
            return reg;
        }

        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
        var window = SDL.SDL_CreateWindow(windowTitle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                                   SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (window == IntPtr.Zero)
        {
            Console.WriteLine("Unable to create a window. SDL. Error: {0}", SDL.SDL_GetError());
            return reg;
        }
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        SDL.SDL_RenderSetLogicalSize(renderer, width, height); // Virtual resolution
        //SDL.SDL_SetWindowResizable(window, SDL.SDL_bool.SDL_TRUE);
        return reg.WithLingoSdlEngine(window, renderer, setup, componentRegistrations);
    }
    public static void Dispose()
    {
        SDL_ttf.TTF_Quit();
        SDL.SDL_AudioQuit();
        SDL.SDL_VideoQuit();
        SDL.SDL_Quit();
    }
    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, nint sdlWindow, nint sdlRenderer, Action<LingoSdlFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
    {
        LingoEngineGlobal.RunFramework = AbstEngineRunFramework.SDL2;
        RegisterServices(reg, setup, sdlWindow, sdlRenderer, componentRegistrations);
        return reg;
    }

    private static void RegisterServices(ILingoEngineRegistration reg, Action<LingoSdlFactory>? setup, nint sdlWindow, nint sdlRenderer, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
    {
        reg
            .ServicesMain(s => s
                    .WithAbstUISdl()
                    .AddSingleton<SdlRootContext>(provider =>
                        new SdlRootContext(
                            sdlWindow,
                            sdlRenderer,
                            provider.GetRequiredService<SdlFocusManager>(),
                            provider.GetRequiredService<IAbstGlobalMouse>(),
                            provider.GetRequiredService<IAbstGlobalKey>()))
                    .AddSingleton<ISdlRootComponentContext>(p => p.GetRequiredService<SdlRootContext>())
                    .AddSingleton<IAbstSDLRootContext>(p => p.GetRequiredService<SdlRootContext>())
                    .AddSingleton<ILingoFrameworkFactory, LingoSdlFactory>()
                )
            .WithFrameworkFactory(setup)
            .AddPreBuildAction(x => x.WithAbstUISdl())
            .AddBuildAction(b =>
            {
                
            });
    }
}
