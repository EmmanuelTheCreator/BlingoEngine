using LingoEngine.Bitmaps;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific toggle button control.
    /// </summary>
    public interface ILingoFrameworkGfxStateButton : ILingoFrameworkGfxNodeInput
    {
        /// <summary>Displayed text on the button.</summary>
        string Text { get; set; }
        /// <summary>Icon texture displayed on the button.</summary>
        ILingoTexture2D? TextureOn { get; set; }
        /// <summary>Whether the button is toggled on.</summary>
        bool IsOn { get; set; }
        ILingoTexture2D? TextureOff { get; set; }
    }
}
