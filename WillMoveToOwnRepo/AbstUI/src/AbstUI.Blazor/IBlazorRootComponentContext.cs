using ImGuiNET;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace AbstUI.Blazor;

public interface IBlazorRootComponentContext
{
    public ImGuiViewportPtr ImGuiViewPort { get; }
    public ImDrawListPtr ImDrawList { get; }
    AbstBlazorComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstMouse AbstMouse { get; }
    IAbstKey AbstKey { get; }

    nint RegisterTexture(nint sdlTexture);
    nint GetTexture(nint textureId);

    APoint GetWindowSize();
}
