using AbstUI.Primitives;
using AbstUI.Inputs;

namespace AbstUI.SDL2.Core;

public interface ISdlRootComponentContext
{
    AbstSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
    IAbstMouse AbstMouse { get; }
    IAbstKey AbstKey { get; }
    SdlFocusManager FocusManager { get; }

    APoint GetWindowSize();
}
