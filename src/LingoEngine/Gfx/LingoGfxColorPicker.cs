using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a color picker input.
    /// </summary>
    public class LingoGfxColorPicker : LingoGfxInputBase<ILingoFrameworkGfxColorPicker>
    {
        public AColor Color { get => _framework.Color; set => _framework.Color = value; }
    }
}
