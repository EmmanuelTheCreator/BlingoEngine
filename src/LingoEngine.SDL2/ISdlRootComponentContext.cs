namespace LingoEngine.SDL2;

public interface ISdlRootComponentContext
{
    LingoSDLComponentContainer ComponentContainer { get; }
    nint Renderer { get; }
}
