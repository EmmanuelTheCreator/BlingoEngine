namespace LingoEngine.SDL2;
using System;
using LingoEngine.Core;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Core;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using LingoEngine.Primitives;
using ImGuiNET;
using System.Numerics;
using LingoEngine.SDL2.Styles;

public class SdlRootContext : IDisposable, ISdlRootComponentContext
{
    private readonly ImGuiSdlBackend _imgui = new();

    private LingoPlayer _lPlayer;
    public nint Window { get; }
    public nint Renderer { get; }


    public LingoDebugOverlay DebugOverlay { get; set; }
    public LingoKey LingoKey { get; }
    internal SdlKey Key { get; }
    public LingoMouse LingoMouse { get; }
    public SdlMouse Mouse { get; }
    private bool _f1Pressed;

    public LingoSDLComponentContainer ComponentContainer { get; } = new();
    internal SdlFactory Factory { get; set; } = null!;

    public ImGuiViewportPtr ImGuiViewPort { get; private set; } = new ImGuiViewportPtr(nint.Zero);
    public ImDrawListPtr ImDrawList { get; private set; } = new ImDrawListPtr(nint.Zero);

    public SdlRootContext(nint window, nint renderer)
    {
        Window = window;
        Renderer = renderer;
        Mouse = new SdlMouse(new Lazy<LingoMouse>(() => LingoMouse!));
        LingoMouse = new LingoMouse(Mouse);
        Key = new SdlKey();
        LingoKey = new LingoKey(Key);
        Key.SetLingoKey(LingoKey);
        _imgui.Init(Window, Renderer);
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
                Mouse.ProcessEvent(e);
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

    public LingoPoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(Window, out var w, out var h);
        return new LingoPoint(w, h);
    }
}
