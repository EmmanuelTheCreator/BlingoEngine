namespace AbstUI.ImGui;

public struct AbstImGuiRenderResult
{
    public bool DoRender { get; set; }
    public nint Texture { get; set; }

    internal static AbstImGuiRenderResult RequireRender() => new AbstImGuiRenderResult
    {
        DoRender = true,
        Texture = nint.Zero
    };

    public static implicit operator AbstImGuiRenderResult(nint texture)
       => new AbstImGuiRenderResult { Texture = texture, DoRender = texture != nint.Zero };

    public static explicit operator nint(AbstImGuiRenderResult r) => r.Texture;
}
