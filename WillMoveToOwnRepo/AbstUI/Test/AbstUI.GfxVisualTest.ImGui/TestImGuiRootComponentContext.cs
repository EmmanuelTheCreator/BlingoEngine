using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;
using AbstUI.ImGui;
using AbstUI.ImGui.Styles;
using AbstUI.ImGui.Inputs;
using LingoEngine.Inputs;


namespace LingoEngine.ImGui2.GfxVisualTest;

public class TestImGuiRootComponentContext : IImGuiRootComponentContext, IDisposable
{



private bool _imguiReady;
    private ImGuiFontManager _fontManager;

    private readonly nint _window;
    public AbstImGuiComponentContainer ComponentContainer { get; } = new();
    public nint Renderer { get; }

    public ImGuiMouse<AbstMouseEvent> Mouse { get; set; }
    public IAbstMouse AbstMouse { get; set; }

    internal ILingoFrameworkKey Key { get; set; }
    public IAbstKey AbstKey { get; set; }
    private readonly ImGuiImGuiBackend _imgui = new();
    public int Width { get; set; }
    public int Height { get; set; }
    public ImGuiViewportPtr ImGuiViewPort { get; private set; } = new ImGuiViewportPtr(nint.Zero);
    public ImDrawListPtr ImDrawList { get; private set; } = new ImDrawListPtr(nint.Zero);

    public nint RegisterTexture(nint ImGuiTexture) => _imgui.RegisterTexture(ImGuiTexture);
    public nint GetTexture(nint textureId) => _imgui.GetTexture(textureId);

    public TestImGuiRootComponentContext()
    {
        ImGui.ImGui_Init(ImGui.ImGui_INIT_VIDEO | ImGui.ImGui_INIT_EVENTS | ImGui.ImGui_INIT_GAMECONTROLLER | ImGui.ImGui_INIT_AUDIO);
        ImGui_ttf.TTF_Init();
        ImGui_image.IMG_Init(ImGui_image.IMG_InitFlags.IMG_INIT_PNG);

        _window = ImGui.ImGui_CreateWindow("ImGui Gfx Visual Test", ImGui.ImGui_WINDOWPOS_CENTERED, ImGui.ImGui_WINDOWPOS_CENTERED, 800, 600,
            ImGui.ImGui_WindowFlags.ImGui_WINDOW_SHOWN | ImGui.ImGui_WindowFlags.ImGui_WINDOW_RESIZABLE);
        Renderer = ImGui.ImGui_CreateRenderer(_window, -1,
            ImGui.ImGui_RendererFlags.ImGui_RENDERER_ACCELERATED | ImGui.ImGui_RendererFlags.ImGui_RENDERER_PRESENTVSYNC);
        ImGui.ImGui_RenderSetLogicalSize(Renderer, Width, Height);
        CreateMouse();
        var key = new LingoImGuiKey();
        Key = key;
        var LingoKey = new LingoKey(key);
        key.SetKeyObj(LingoKey);

        _imgui.Init(_window, Renderer);
        _fontManager = new ImGuiFontManager();
        _fontManager.LoadAll();
        _imguiReady = true;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            while (ImGui.ImGui_PollEvent(out var e) == 1)
            {
                _imgui.ProcessEvent(ref e);
                if (e.type == ImGui.ImGui_EventType.ImGui_QUIT)
                    running = false;
            }

            ImGuiViewPort = _imgui.BeginFrame();
            ImDrawList = ImGui.GetForegroundDrawList();
            ImDrawList.AddText(ImGuiViewPort.WorkPos + new System.Numerics.Vector2(10, 10), 0xFFFFFFFF, "Overlay text");
            var origin = ImGuiViewPort.WorkPos;
            //RenderImGuiOverlay2(); // () => { });

            // --- Your ImGui code (safe for SetCursorPos) ---
            //ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.Once);
            //ImGui.Begin("Debug2");
            //ImGui.SetCursorPos(new System.Numerics.Vector2(10, 40)); // <- no AccessViolation now
            //ImGui.Text("Hello from ImGui.NET (ImGui_Renderer)");



            ImGui.ImGui_SetRenderDrawColor(Renderer, 50, 0, 50, 255);
            ImGui.ImGui_RenderClear(Renderer);
            
            ComponentContainer.Render(new AbstImGuiRenderContext(Renderer,ImGuiViewPort,ImDrawList, origin, _fontManager));


            _imgui.EndFrame();  // draws ImGui on top

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
            ImGuiWindowFlags.NoBackground; // keep ImGui clear color

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
        Mouse = new LingoImGuiMouse(new Lazy<AbstMouse<Events.LingoMouseEvent>>(() => (AbstMouse < Events.LingoMouseEvent > )AbstMouse));
        var mouseImpl = Mouse;
        var mouse = new LingoMouse(mouseImpl);
        mouseImpl.SetMouse(mouse);
        AbstMouse = mouse;
        return mouse;
    }

    public void Dispose()
    {
        _imgui.Shutdown();
        ImGui.ImGui_DestroyRenderer(Renderer);
        ImGui.ImGui_DestroyWindow(_window);
        ImGui_ttf.TTF_Quit();
        ImGui_image.IMG_Quit();
        ImGui.ImGui_Quit();
    }

    public APoint GetWindowSize()
    {
        ImGui.ImGui_GetWindowSize(_window, out var w, out var h);
        return new APoint(w, h);
    }
}
