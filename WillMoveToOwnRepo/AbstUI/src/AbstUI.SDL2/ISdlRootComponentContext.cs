using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace AbstUI.SDL2;

public interface ISdlRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    AbstSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstMouse AbstMouse { get; }
    IAbstKey AbstKey { get; }

    nint RegisterTexture(nint sdlTexture);
    nint GetTexture(nint textureId);

    APoint GetWindowSize();
}
