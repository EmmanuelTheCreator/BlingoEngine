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
    private LingoSdlMouse _sdlMouse;
    private LingoPlayer _lPlayer;
    private LingoClock _clock;
    public LingoDebugOverlay DebugOverlay { get; set; }
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
        bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
        if (f1 && !_f1Pressed)
            DebugOverlay.Toggle();
        _f1Pressed = f1;
        _clock.Tick(delta);
    }
}
