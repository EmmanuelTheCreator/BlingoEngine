using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace AbstUI.ImGui;

public interface IImGuiRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    AbstImGuiComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstMouse AbstMouse { get; }
    IAbstKey AbstKey { get; }

    nint RegisterTexture(nint sdlTexture);
    nint GetTexture(nint textureId);

    APoint GetWindowSize();
}
