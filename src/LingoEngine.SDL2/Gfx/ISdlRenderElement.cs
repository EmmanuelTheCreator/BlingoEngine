namespace LingoEngine.SDL2.Gfx
{
    /// <summary>
    /// Common interface for SDL ImGui-rendered elements.
    /// </summary>
    internal interface ISdlRenderElement
    {
        /// <summary>Render the element using the active ImGui context.</summary>
        void Render();
    }
}
