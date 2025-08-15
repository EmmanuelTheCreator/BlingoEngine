using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace LingoEngine.SDL2;

public interface ISdlRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    LingoSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstUIMouse LingoMouse { get; }
    IAbstUIKey LingoKey { get; }

    nint RegisterTexture(nint sdlTexture);
    nint GetTexture(nint textureId);

    APoint GetWindowSize();
}
