namespace LingoEngine.SDL2;
using AbstUI.Inputs;
using AbstUI.SDL2;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.SDL2.Core;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using System;

public class LingoSdlRootContext : AbstUISdlRootContext<LingoMouse>
{
    private bool _f1Pressed;
    private LingoSdlMouse _sdlMouse = null!;
    private LingoPlayer _lPlayer = null!;
    private LingoClock _clock = null!;
    public LingoDebugOverlay DebugOverlay { get; set; } = null!;

    private Lazy<SdlStage> _stage;

    public LingoSdlKey Key { get; set; }
    public IAbstFrameworkMouse Mouse { get; set; }
    internal LingoSdlFactory LingoFactory { get; set; } = null!;
    public LingoSdlRootContext(nint window, nint renderer, SdlFocusManager focusManager, IAbstGlobalMouse globalMouse, IAbstGlobalKey globalKey)
        : base(window, renderer, focusManager)
    {
        GlobalMouse = globalMouse;
        GlobalKey = globalKey;

        _sdlMouse = new LingoSdlMouse(new Lazy<AbstMouse<LingoMouseEvent>>(() => (LingoMouse)AbstMouse));
        Mouse = _sdlMouse;
        AbstMouse = new LingoMouse(_sdlMouse);

        Key = new LingoSdlKey();
        var lingoKey = new LingoKey(Key);
        AbstKey = lingoKey;
        Key.SetKeyObj(lingoKey);
        _stage = new Lazy<SdlStage>(() => _lPlayer.Stage.Framework<SdlStage>());
    }

    public void Init(ILingoPlayer player)
    {
        _lPlayer = (LingoPlayer)player;
        _clock = (LingoClock)_lPlayer.Clock;
        var overlay = new SdlDebugOverlay(LingoFactory);
        ComponentContainer.Activate(overlay.ComponentContext);
        DebugOverlay = new LingoDebugOverlay(overlay, _lPlayer);
       
    }

    protected override void HandleEvent(SDL.SDL_Event e, ref bool running)
    {
        Key.ProcessEvent(e);
        _sdlMouse.ProcessEvent(e);
    }

    protected override void Update(float delta)
    {
        DebugOverlay.Update(delta);
        DebugOverlay.Render();
        bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
        if (f1 && !_f1Pressed)
            DebugOverlay.Toggle();
        _f1Pressed = f1;
        _clock.Tick(delta);

    }

    protected override void Render()
    {
        //SDL.SDL_SetRenderDrawColor(Renderer, 255, 0, 0, 255);
        //SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 100, y = 100, w = 200, h = 150 };
        //SDL.SDL_RenderFillRect(Renderer, ref rect);
        var stageTexture = _stage.Value.LastTexture;
        SDL.SDL_SetRenderTarget(Renderer, nint.Zero);

        SDL.SDL_RenderCopy(Renderer, stageTexture, IntPtr.Zero, IntPtr.Zero);
        ComponentContainer.Render(Factory.CreateRenderContext(null, System.Numerics.Vector2.Zero));
        SDL.SDL_RenderPresent(Renderer);
        
    }
}
