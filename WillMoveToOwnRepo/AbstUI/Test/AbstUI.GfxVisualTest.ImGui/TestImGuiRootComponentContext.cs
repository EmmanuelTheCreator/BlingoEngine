using AbstUI.Inputs;
using AbstUI.ImGui;
using AbstUI.ImGui.Styles;
using AbstUI.Primitives;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.ImGui.Inputs;

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

    public TestImGuiRootComponentContext()
    {
        _imgui.Init(nint.Zero, nint.Zero);
        var io = global::ImGuiNET.ImGui.GetIO();
        io.DisplaySize = new Vector2(800, 600);
        io.Fonts.AddFontDefault();
        io.Fonts.Build();
        FontManager.LoadAll();

        CreateMouse();
        var key = new LingoImGuiKey();
        Key = key;
        var lingoKey = new LingoKey(key);
        key.SetKeyObj(lingoKey);
        AbstKey = lingoKey;
    }

    /// <summary>Font manager used by the visual test.</summary>
    public ImGuiFontManager FontManager { get; } = new();

    public AbstImGuiComponentContainer ComponentContainer { get; } = new();
    public ImGuiViewportPtr ImGuiViewPort { get; private set; }
    public ImDrawListPtr ImDrawList { get; private set; }
    public nint Renderer { get; } = nint.Zero;
    public LingoImGuiMouse Mouse { get; private set; } = null!;
    public IAbstMouse AbstMouse { get; private set; } = null!;
    internal ILingoFrameworkKey Key { get; private set; } = null!;
    public IAbstKey AbstKey { get; private set; } = null!;

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

    private LingoMouse CreateMouse()
    {
        Mouse = new LingoImGuiMouse(new Lazy<AbstMouse<LingoMouseEvent>>(() => (LingoMouse)AbstMouse));
        var lingoMouse = new LingoMouse(Mouse);
        Mouse.ReplaceMouseObj(lingoMouse);
        AbstMouse = lingoMouse;
        return lingoMouse;
    }
}

