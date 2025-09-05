using System;
using System.IO;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;

namespace AbstUI.SDLTest;

public static class SdlTestHost
{
    public static void Run(Action<IntPtr, IntPtr, SdlFontManager> test)
    {
        Directory.CreateDirectory("C:/temp/director");

        SDL.SDL_SetHint("SDL_RENDER_DRIVER", "software");
        SDL.SDL_SetHint("SDL_VIDEODRIVER", "dummy");
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            throw new Exception(SDL.SDL_GetError());
        if (SDL_ttf.TTF_Init() != 0)
            throw new Exception(SDL.SDL_GetError());

        var window = SDL.SDL_CreateWindow("test", 0, 0, 64, 64, SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, 0);

        var fontManager = new SdlFontManager();
        fontManager.LoadAll();

        try
        {
            test(window, renderer, fontManager);
        }
        finally
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }
    }
}
