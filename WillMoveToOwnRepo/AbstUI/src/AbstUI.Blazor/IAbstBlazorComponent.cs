namespace AbstUI.Blazor;

public interface IAbstBlazorComponent
{
    /// <summary>
    /// Renders the component and returns the texture handle if one was produced.
    /// </summary>
    /// <param name="context">Render context providing the Blazor renderer.</param>
    /// <returns>The texture handle or <c>nint.Zero</c> when no texture is created.</returns>
    AbstBlazorRenderResult Render(AbstBlazorRenderContext context);
}
