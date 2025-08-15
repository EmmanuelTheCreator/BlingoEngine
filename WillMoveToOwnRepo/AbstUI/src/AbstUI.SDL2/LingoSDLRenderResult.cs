namespace AbstUI.SDL2;

public struct LingoSDLRenderResult
{
    public bool DoRender { get; set; }
    public nint Texture { get; set; }

    internal static LingoSDLRenderResult RequireRender() => new LingoSDLRenderResult
    {
        DoRender = true,
        Texture = nint.Zero
    };

    public static implicit operator LingoSDLRenderResult(nint texture)
       => new LingoSDLRenderResult { Texture = texture, DoRender = texture != nint.Zero };

    public static explicit operator nint(LingoSDLRenderResult r) => r.Texture;
}
