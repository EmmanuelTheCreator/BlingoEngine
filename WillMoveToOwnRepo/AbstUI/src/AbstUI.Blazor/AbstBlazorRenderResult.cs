namespace AbstUI.Blazor;

public struct AbstBlazorRenderResult
{
    public bool DoRender { get; set; }
    public nint Texture { get; set; }

    internal static AbstBlazorRenderResult RequireRender() => new AbstBlazorRenderResult
    {
        DoRender = true,
        Texture = nint.Zero
    };

    public static implicit operator AbstBlazorRenderResult(nint texture)
       => new AbstBlazorRenderResult { Texture = texture, DoRender = texture != nint.Zero };

    public static explicit operator nint(AbstBlazorRenderResult r) => r.Texture;
}
