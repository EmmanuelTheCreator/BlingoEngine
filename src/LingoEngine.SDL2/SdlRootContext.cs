namespace LingoEngine.SDL2;
using System;
using System.Collections.Generic;
using LingoEngine.Core;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Core;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2;

public class SdlRootContext : IDisposable
{
    public nint Window { get; }
    public nint Renderer { get; }

    private LingoPlayer _lPlayer;

    public LingoDebugOverlay DebugOverlay { get; set; }
    internal List<SdlKey> Keys { get; } = new() { new SdlKey() };
    internal List<SdlMouse> Mice { get; } = new() { new SdlMouse(new Lazy<LingoMouse>(() => null!)) };
    internal SdlKey Key => Keys[0];
    internal SdlMouse Mouse => Mice[0];
    private bool _f1Pressed;
    public LingoSDLComponentContainer ComponentContainer { get; } = new();
    internal SdlFactory Factory { get; set; } = null!;

    public SdlRootContext(nint window, nint renderer)
    {
        Window = window;
        Renderer = renderer;

    }
    public void Init(ILingoPlayer player)
    {
        _lPlayer = (LingoPlayer)player;
        var overlay = new SdlDebugOverlay(Factory);
        Factory.ComponentContainer.Activate(overlay.ComponentContext);
        DebugOverlay = new LingoDebugOverlay(overlay, _lPlayer);
    }
    public void Run()
    {
        var clock = (LingoClock)_lPlayer.Clock;

        bool running = true;
        uint last = SDL.SDL_GetTicks();
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
                foreach (var k in Keys)
                    k.ProcessEvent(e);
                foreach (var m in Mice)
                    m.ProcessEvent(e);
            }
            uint now = SDL.SDL_GetTicks();
            float delta = (now - last) / 1000f;
            last = now;
            DebugOverlay.Update(delta);
            bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
            if (f1 && !_f1Pressed)
                DebugOverlay.Toggle();
            _f1Pressed = f1;
            clock.Tick(delta);
        }
        Dispose();
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
}
