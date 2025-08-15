namespace AbstUI.SDL2;

public interface IAbstSDLComponent
{
    /// <summary>
    /// Renders the component and returns the texture handle if one was produced.
    /// </summary>
    /// <param name="context">Render context providing the SDL renderer.</param>
    /// <returns>The texture handle or <c>nint.Zero</c> when no texture is created.</returns>
    AbstSDLRenderResult Render(AbstSDLRenderContext context);
}
