namespace LingoEngine.SDL2;

public class LingoSDLRenderContext
{
    public nint Renderer { get; }
    public LingoSDLRenderContext(nint renderer)
    {
        Renderer = renderer;
    }
}
