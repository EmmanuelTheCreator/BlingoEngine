using ImGuiNET;
using LingoEngine.Inputs;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2;

public interface ISdlRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    LingoSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    LingoMouse LingoMouse { get; }
    LingoKey LingoKey { get; }

    LingoPoint GetWindowSize();
}
