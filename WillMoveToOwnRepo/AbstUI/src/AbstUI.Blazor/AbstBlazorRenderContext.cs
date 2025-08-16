using AbstUI.Blazor.Styles;
using ImGuiNET;

namespace AbstUI.Blazor;
public class AbstBlazorRenderContext
{
    public nint Renderer { get; }
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    public System.Numerics.Vector2 Origin { get; }
    public BlazorFontManager BlazorFontManager { get; }

    public AbstBlazorRenderContext(nint renderer, ImGuiViewportPtr imGuiViewPort, ImDrawListPtr imDrawList, System.Numerics.Vector2 origin, BlazorFontManager sdlFontManager)
    {
        Renderer = renderer;
        ImGuiViewPort = imGuiViewPort;
        ImDrawList = imDrawList;
        Origin = origin;
        BlazorFontManager = sdlFontManager;
    }
    public ImFontPtr? GetFont(int size) => BlazorFontManager.GetFont(size);

    public AbstBlazorRenderContext CreateNew(System.Numerics.Vector2 childOrigin)
    {
        var childCtx = new AbstBlazorRenderContext(
                Renderer,
                ImGuiViewPort,
                ImGui.GetWindowDrawList(),
                childOrigin, BlazorFontManager);
        return childCtx;
    }
}
