using AbstUI.Primitives;
using AbstUI.Inputs;

namespace AbstUI.SDL2;

public interface ISdlRootComponentContext
{
    AbstSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstMouse AbstMouse { get; }
    IAbstKey AbstKey { get; }

    APoint GetWindowSize();
}
