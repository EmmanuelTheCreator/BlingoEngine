namespace LingoEngine.SDL2;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;
using System;

public abstract class AbstUISdlRootContext<TMouse> : IAbstSDLRootContext, IDisposable
     where TMouse : IAbstMouse
{
    private IAbstGlobalMouse _globalMouse;
    protected AbstGodotGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent>? _frameworkMouse;

    public nint Window { get; }
    public nint Renderer { get; }
    public IAbstGlobalMouse GlobalMouse
    {
        get => _globalMouse;
        set
        {
            _globalMouse = value;
            _frameworkMouse = ((GlobalSDLAbstMouse)_globalMouse).Framework<AbstGodotGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent>>();
        }
    }



    public SdlFocusManager FocusManager { get; }
    public AbstSDLComponentContainer ComponentContainer { get; }
    

    public AbstUISdlRootContext(nint window, nint renderer, SdlFocusManager focusManager)
    {
        Window = window;
        Renderer = renderer;
        FocusManager = focusManager;
        ComponentContainer = new AbstSDLComponentContainer(focusManager);
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
