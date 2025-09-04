namespace AbstUI.SDL2.Core;

public struct AbstSDLRenderResult
{
    public bool DoRender { get; set; }
    public nint Texture { get; set; }

    internal static AbstSDLRenderResult RequireRender() => new AbstSDLRenderResult
    {
        DoRender = true,
        Texture = nint.Zero
    };

    public static implicit operator AbstSDLRenderResult(nint texture)
       => new AbstSDLRenderResult { Texture = texture, DoRender = false };

    public static explicit operator nint(AbstSDLRenderResult r) => r.Texture;
}
