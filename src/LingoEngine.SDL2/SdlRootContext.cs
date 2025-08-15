namespace LingoEngine.SDL2;
using System;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using ImGuiNET;
using AbstUI.Inputs;
using LingoEngine.Events;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

public class SdlRootContext : AbstUISdlRootContext<LingoMouse> , ISdlRootComponentContext
{
    private bool _f1Pressed;
    private LingoSdlMouse _sdlMouse;
    protected readonly ImGuiSdlBackend _imgui = new();
    private LingoPlayer _lPlayer;
    public LingoDebugOverlay DebugOverlay { get; set; }
    public nint RegisterTexture(nint sdlTexture) => _imgui.RegisterTexture(sdlTexture);
    public nint GetTexture(nint textureId) => _imgui.GetTexture(textureId);
   
    public ImGuiViewportPtr ImGuiViewPort { get; private set; } = new ImGuiViewportPtr(nint.Zero);
    public ImDrawListPtr ImDrawList { get; private set; } = new ImDrawListPtr(nint.Zero);

    public SdlRootContext(nint window, nint renderer) : base(window, renderer)
    {
        _sdlMouse = new LingoSdlMouse(new Lazy<AbstMouse<LingoMouseEvent>>(() => (LingoMouse)AbstMouse));
        Mouse = _sdlMouse;
        AbstMouse = new LingoMouse(_sdlMouse);
        _imgui.Init(Window, Renderer);

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
                _imgui.ProcessEvent(ref e);
                Key.ProcessEvent(e);
                _sdlMouse.ProcessEvent(e);
            }
            uint now = SDL.SDL_GetTicks();
            float delta = (now - last) / 1000f;
            last = now;
            ImGuiViewPort = _imgui.BeginFrame();
            ImDrawList = ImGui.GetWindowDrawList();

            DebugOverlay.Update(delta);
            bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
            if (f1 && !_f1Pressed)
                DebugOverlay.Toggle();
            _f1Pressed = f1;
            clock.Tick(delta);
            _imgui.EndFrame();
        }
        Dispose();
    }
}
