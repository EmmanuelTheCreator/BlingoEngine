using ImGuiNET;
using AbstUI.ImGui.Styles;

namespace AbstUI.ImGui;
public class AbstImGuiRenderContext
{
    public nint Renderer { get; }
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    public System.Numerics.Vector2 Origin { get; }
    public ImGuiFontManager ImGuiFontManager { get; }

    public AbstImGuiRenderContext(nint renderer, ImGuiViewportPtr imGuiViewPort, ImDrawListPtr imDrawList, System.Numerics.Vector2 origin, ImGuiFontManager sdlFontManager)
    {
        Renderer = renderer;
        ImGuiViewPort = imGuiViewPort;
        ImDrawList = imDrawList;
        Origin = origin;
        ImGuiFontManager = sdlFontManager;
    }

    public ImFontPtr? GetFont(int size) => ImGuiFontManager.GetFont(size);

    public AbstImGuiRenderContext CreateNew(System.Numerics.Vector2 childOrigin)
    {
        var childCtx = new AbstImGuiRenderContext(
                Renderer,
                ImGuiViewPort,
                global::ImGuiNET.ImGui.GetWindowDrawList(),
                childOrigin, ImGuiFontManager);
        return childCtx;
    }
}
