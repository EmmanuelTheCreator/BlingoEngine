using ImGuiNET;
using LingoEngine.SDL2.Styles;

namespace LingoEngine.SDL2;
public class LingoSDLRenderContext
{
    public nint Renderer { get; }
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    public System.Numerics.Vector2 Origin { get; }
    public SdlFontManager SdlFontManager { get; }

    public LingoSDLRenderContext(nint renderer, ImGuiViewportPtr imGuiViewPort, ImDrawListPtr imDrawList, System.Numerics.Vector2 origin, SdlFontManager sdlFontManager)
    {
        Renderer = renderer;
        ImGuiViewPort = imGuiViewPort;
        ImDrawList = imDrawList;
        Origin = origin;
        SdlFontManager = sdlFontManager;
    }
    public ImFontPtr? GetFont(int size) => SdlFontManager.GetFont(size);

    public LingoSDLRenderContext CreateNew(System.Numerics.Vector2 childOrigin)
    {
        var childCtx = new LingoSDLRenderContext(
                Renderer,
                ImGuiViewPort,
                ImGui.GetWindowDrawList(),
                childOrigin, SdlFontManager);
        return childCtx;
    }
}
