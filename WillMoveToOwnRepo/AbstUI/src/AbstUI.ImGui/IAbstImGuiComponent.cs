namespace AbstUI.ImGui;

public interface IAbstImGuiComponent
{
    /// <summary>
    /// Renders the component and returns the texture handle if one was produced.
    /// </summary>
    /// <param name="context">Render context for the current frame.</param>
    /// <returns>The texture handle or <c>nint.Zero</c> when no texture is created.</returns>
    AbstImGuiRenderResult Render(AbstImGuiRenderContext context);
}
