namespace LingoEngine.SDL2;
using System;
using LingoEngine.Core;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Core;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.Stages;
using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;
using LingoEngine.SDL2.Movies;
using LingoEngine.Events;

public class SdlRootContext : AbstUISdlRootContext<LingoMouse> , ISdlRootComponentContext
{
    private bool _f1Pressed;
    private LingoSdlMouse _sdlMouse;
    private readonly ImGuiSdlBackend _imgui = new();
    private LingoPlayer _lPlayer;
    public LingoDebugOverlay DebugOverlay { get; set; }
    public nint RegisterTexture(nint sdlTexture) => _imgui.RegisterTexture(sdlTexture);
    public nint GetTexture(nint textureId) => _imgui.GetTexture(textureId);
   
    public ImGuiViewportPtr ImGuiViewPort { get; private set; } = new ImGuiViewportPtr(nint.Zero);
    public ImDrawListPtr ImDrawList { get; private set; } = new ImDrawListPtr(nint.Zero);

    public SdlRootContext(nint window, nint renderer) : base(window, renderer)
    {
        _sdlMouse = new LingoSdlMouse(new Lazy<AbstUIMouse<LingoMouseEvent>>(() => (LingoMouse)LingoMouse));
        Mouse = _sdlMouse;
        LingoMouse = new LingoMouse(_sdlMouse);
        _imgui.Init(Window, Renderer);

        Key = new SdlKey();
        var lingoKey = new LingoKey(Key);
        LingoKey = lingoKey;
        Key.SetLingoKey(lingoKey);
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


public abstract class AbstUISdlRootContext<TMouse> : IDisposable
     where TMouse : IAbstUIMouse
{
    
    public nint Window { get; }
    public nint Renderer { get; }

    public SdlKey Key { get; set; }
    public IAbstUIFrameworkMouse Mouse { get; set; }

    public IAbstUIKey LingoKey { get; protected set; }
    public IAbstUIMouse LingoMouse { get; set; }
   
    

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
