using AbstUI.Inputs;
using AbstUI.ImGui;
using AbstUI.ImGui.Styles;
using AbstUI.Primitives;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading;
using AbstUI.ImGui.Inputs;

namespace LingoEngine.ImGui.GfxVisualTest;

/// <summary>
/// Minimal runtime environment used by the ImGui graphics visual test.
/// It provides the <see cref="IImGuiRootComponentContext"/> required by
/// <see cref="AbstImGuiComponentFactory"/> without relying on a specific
/// windowing or input backend.
/// </summary>
public sealed class TestImGuiRootComponentContext : IImGuiRootComponentContext, IDisposable
{
    private readonly ImGuiImGuiBackend _imgui = new();

   
    /// <summary>Font manager used by the visual test.</summary>
    public ImGuiFontManager FontManager { get; } = new();

    public AbstImGuiComponentContainer ComponentContainer { get; } = new();
    public ImGuiViewportPtr ImGuiViewPort { get; private set; }
    public ImDrawListPtr ImDrawList { get; private set; }
    public nint Renderer { get; } = nint.Zero;
    public AbstImGuiMouse<AbstMouseEvent> Mouse { get; private set; } = null!;
    public IAbstMouse AbstMouse { get; private set; } = null!;
    internal IAbstFrameworkKey Key { get; private set; } = null!;
    public IAbstKey AbstKey { get; private set; } = null!;


    public TestImGuiRootComponentContext()
    {
        _imgui.Init(nint.Zero, nint.Zero);
        var io = global::ImGuiNET.ImGui.GetIO();
        io.DisplaySize = new Vector2(800, 600);
        io.Fonts.AddFontDefault();
        io.Fonts.Build();
        FontManager.LoadAll();

        Mouse = new AbstImGuiMouse<AbstMouseEvent>(new Lazy<AbstMouse<AbstMouseEvent>>(() => (AbstMouse<AbstMouseEvent>)AbstMouse));
        var lingoMouse = new AbstMouse(Mouse);
        Mouse.ReplaceMouseObj(lingoMouse);
        AbstMouse = lingoMouse;

        var key = new AbstImGuiKey();
        Key = key;
        var lingoKey = new AbstKey(key);
        AbstKey = lingoKey;
    }


    /// <summary>
    /// Runs an ImGui frame loop until a key is pressed.
    /// This allows the visual test to display its components on screen.
    /// </summary>
    public void Run()
    {
        while (!Console.KeyAvailable)
        {
            ImGuiViewPort = _imgui.BeginFrame();
            ImDrawList = global::ImGuiNET.ImGui.GetForegroundDrawList();
            ImDrawList.AddText(ImGuiViewPort.WorkPos + new Vector2(10, 10), 0xFFFFFFFF, "ImGui visual test");
            var origin = ImGuiViewPort.WorkPos;
            ComponentContainer.Render(new AbstImGuiRenderContext(Renderer, ImGuiViewPort, ImDrawList, origin, FontManager));
            _imgui.EndFrame();
            Thread.Sleep(16);
        }
    }

    public nint RegisterTexture(nint texture) => _imgui.RegisterTexture(texture);
    public nint GetTexture(nint textureId) => _imgui.GetTexture(textureId);
    public APoint GetWindowSize() => new(800, 600);

    public void Dispose() => _imgui.Dispose();

 
}

