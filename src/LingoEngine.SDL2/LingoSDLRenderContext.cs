namespace LingoEngine.SDL2;
public class LingoSDLRenderContext
{
    public nint Renderer { get; }
    public ImGuiNET.ImGuiViewportPtr ImGuiViewPort { get; }
    public ImGuiNET.ImDrawListPtr ImDrawList { get; }
    public System.Numerics.Vector2 Origin { get; }
    public LingoSDLRenderContext(nint renderer, ImGuiNET.ImGuiViewportPtr imGuiViewPort, ImGuiNET.ImDrawListPtr imDrawList, System.Numerics.Vector2 origin)
    {
        Renderer = renderer;
        ImGuiViewPort = imGuiViewPort;
        ImDrawList = imDrawList;
        Origin = origin;
    }
}
