using LingoEngine.Bitmaps;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a state (toggle) button.
    /// </summary>
    public class LingoGfxStateButton : LingoGfxInputBase<ILingoFrameworkGfxStateButton>
    {
        /// <summary>Displayed text on the button.</summary>
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        /// <summary>Icon texture displayed on the button.</summary>
        public Bitmaps.ILingoTexture2D? TextureOn { get => _framework.TextureOn; set => _framework.TextureOn = value; }
        /// <summary>Current toggle state.</summary>
        public bool IsOn { get => _framework.IsOn; set => _framework.IsOn = value; }
        public ILingoTexture2D? TextureOff { get => _framework.TextureOff; set => _framework.TextureOff = value; }
    }
}
