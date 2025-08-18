namespace LingoEngine.SDL2;
using System;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using AbstUI.Inputs;
using LingoEngine.Events;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

public class SdlRootContext : AbstUISdlRootContext<LingoMouse>, ISdlRootComponentContext
{
    private bool _f1Pressed;
    private LingoSdlMouse _sdlMouse;
    private LingoPlayer _lPlayer;
    public LingoDebugOverlay DebugOverlay { get; set; }

    public SdlRootContext(nint window, nint renderer) : base(window, renderer)
    {
        _sdlMouse = new LingoSdlMouse(new Lazy<AbstMouse<LingoMouseEvent>>(() => (LingoMouse)AbstMouse));
        Mouse = _sdlMouse;
        AbstMouse = new LingoMouse(_sdlMouse);

        Key = new LingoSdlKey();
        var lingoKey = new LingoKey(Key);
        AbstKey = lingoKey;
        Key.SetKeyObj(lingoKey);
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
                Key.ProcessEvent(e);
                _sdlMouse.ProcessEvent(e);
                Factory.ComponentContainer.HandleEvent(e);
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

            //SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
            //SDL.SDL_RenderClear(Renderer);
            Factory.ComponentContainer.Render(Factory.CreateRenderContext());
            SDL.SDL_RenderPresent(Renderer);
        }
        Dispose();
    }
}
