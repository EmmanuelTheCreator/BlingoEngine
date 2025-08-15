namespace LingoEngine.SDL2;
using System;
using LingoEngine.SDL2.Inputs;
using AbstUI.Primitives;
using AbstUI.Inputs;
using LingoEngine.SDL2.Core;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2;

public abstract class AbstUISdlRootContext<TMouse> : IDisposable
     where TMouse : IAbstMouse
{
    
    public nint Window { get; }
    public nint Renderer { get; }

    public LingoSdlKey Key { get; set; }
    public IAbstFrameworkMouse Mouse { get; set; }

    public IAbstKey LingoKey { get; protected set; }
    public IAbstMouse LingoMouse { get; set; }
   
    

    public LingoSDLComponentContainer ComponentContainer { get; } = new();
    internal LingoSdlFactory Factory { get; set; } = null!;

  
    public AbstUISdlRootContext(nint window, nint renderer)
    {
        Window = window;
        Renderer = renderer;
    }
   
   



    public void Dispose()
    {
        if (Renderer != nint.Zero)
        {
            SDL.SDL_DestroyRenderer(Renderer);
        }
        if (Window != nint.Zero)
        {
            SDL.SDL_DestroyWindow(Window);
        }
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }

    public APoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(Window, out var w, out var h);
        return new APoint(w, h);
    }
}
