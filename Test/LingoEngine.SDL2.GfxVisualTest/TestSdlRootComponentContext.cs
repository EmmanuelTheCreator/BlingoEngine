using ImGuiNET;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.SDL2;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Styles;
using LingoEngine.Stages;
using System.Numerics;


namespace LingoEngine.SDL2.GfxVisualTest;

public class TestSdlRootComponentContext : ISdlRootComponentContext, IDisposable
{



private bool _imguiReady;
    private SdlFontManager _fontManager;

    private readonly nint _window;
    public LingoSDLComponentContainer ComponentContainer { get; } = new();
    public nint Renderer { get; }

    public SdlMouse Mouse { get; set; }
    public LingoMouse LingoMouse { get; set; }

    internal SdlKey Key { get; set; } = new Inputs.SdlKey();
    public LingoKey LingoKey { get; set; }
    private readonly ImGuiSdlBackend _imgui = new();
    public int Width { get; set; }
    public int Height { get; set; }
    public ImGuiViewportPtr ImGuiViewPort { get; private set; }
    public ImDrawListPtr ImDrawList { get; private set; }

    public nint RegisterTexture(nint sdlTexture) => _imgui.RegisterTexture(sdlTexture);

    public TestSdlRootComponentContext()
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_AUDIO);
        SDL_ttf.TTF_Init();
        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);

        _window = SDL.SDL_CreateWindow("SDL Gfx Visual Test", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 800, 600,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        Renderer = SDL.SDL_CreateRenderer(_window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        SDL.SDL_RenderSetLogicalSize(Renderer, Width, Height);
        CreateMouse();
        LingoKey = new LingoKey(Key);
        Key.SetLingoKey(LingoKey);

        _imgui.Init(_window, Renderer);
        _fontManager = new Styles.SdlFontManager();
        _fontManager.LoadAll();
        _imguiReady = true;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                _imgui.ProcessEvent(ref e);
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
            }

            ImGuiViewPort = _imgui.BeginFrame();
            ImDrawList = ImGui.GetForegroundDrawList();
            //ImDrawList.AddText(ImGuiViewPort.WorkPos + new System.Numerics.Vector2(10, 10), 0xFFFFFFFF, "Overlay text");

            //RenderImGuiOverlay2(); // () => { });

            // --- Your ImGui code (safe for SetCursorPos) ---
            //ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.Once);
            //ImGui.Begin("Debug2");
            //ImGui.SetCursorPos(new System.Numerics.Vector2(10, 40)); // <- no AccessViolation now
            //ImGui.Text("Hello from ImGui.NET (SDL_Renderer)");



            SDL.SDL_SetRenderDrawColor(Renderer, 50, 0, 50, 255);
            SDL.SDL_RenderClear(Renderer);
   
            ComponentContainer.Render(new LingoSDLRenderContext(Renderer,ImGuiViewPort,ImDrawList, ImGuiViewPort.WorkPos, _fontManager));


            _imgui.EndFrame();  // draws ImGui on top

            SDL.SDL_RenderPresent(Renderer);
        }
    }
    private void RenderImGuiOverlay(Action render)
    {
        var vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.WorkPos);
        ImGui.SetNextWindowSize(vp.WorkSize);

        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav |
            ImGuiWindowFlags.NoBackground; // keep SDL clear color

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
        if (ImGui.Begin("##overlay_root", flags))
        {
            // now SetCursorPos is valid:
            ImGui.SetCursorPos(new System.Numerics.Vector2(10, 10));
            render();
            // ... your widgets ...
            // ImGui.InputText("Name", ref _name, 64);
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
    private void RenderImGuiOverlay2()
    {
        var vp = ImGui.GetMainViewport();
        var dl = ImGui.GetForegroundDrawList();
        // draw directly (no widgets). Example:
        dl.AddText(vp.WorkPos + new System.Numerics.Vector2(10, 10), 0xFFFFFFFF, "Overlay text");
    }



    public LingoMouse CreateMouse()
    {
        Mouse = new SdlMouse(new Lazy<LingoMouse>(() => LingoMouse));
        var mouseImpl = Mouse;
        var mouse = new LingoMouse(mouseImpl);
        mouseImpl.SetLingoMouse(mouse);
        LingoMouse = mouse;
        return mouse;
    }

    public void Dispose()
    {
        _imgui.Shutdown();
        SDL.SDL_DestroyRenderer(Renderer);
        SDL.SDL_DestroyWindow(_window);
        SDL_ttf.TTF_Quit();
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }

    public LingoPoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(_window, out var w, out var h);
        return new LingoPoint(w, h);
    }
}
