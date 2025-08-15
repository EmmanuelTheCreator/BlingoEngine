using AbstUI.SDL2.Styles;
using ImGuiNET;

namespace AbstUI.SDL2;
public class AbstSDLRenderContext
{
    public nint Renderer { get; }
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    public System.Numerics.Vector2 Origin { get; }
    public SdlFontManager SdlFontManager { get; }

    public AbstSDLRenderContext(nint renderer, ImGuiViewportPtr imGuiViewPort, ImDrawListPtr imDrawList, System.Numerics.Vector2 origin, SdlFontManager sdlFontManager)
    {
        Renderer = renderer;
        ImGuiViewPort = imGuiViewPort;
        ImDrawList = imDrawList;
        Origin = origin;
        SdlFontManager = sdlFontManager;
    }
    public ImFontPtr? GetFont(int size) => SdlFontManager.GetFont(size);

    public AbstSDLRenderContext CreateNew(System.Numerics.Vector2 childOrigin)
    {
        var childCtx = new AbstSDLRenderContext(
                Renderer,
                ImGuiViewPort,
                ImGui.GetWindowDrawList(),
                childOrigin, SdlFontManager);
        return childCtx;
    }
}
