using ImGuiNET;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Inputs;

namespace LingoEngine.SDL2;

public interface ISdlRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    LingoSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    LingoMouse LingoMouse { get; }
    LingoKey LingoKey { get; }

    nint RegisterTexture(nint sdlTexture);
    nint GetTexture(nint textureId);

    APoint GetWindowSize();
}
