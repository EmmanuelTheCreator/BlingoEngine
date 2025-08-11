using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific label used for displaying text.
    /// </summary>
    public interface ILingoFrameworkGfxLabel : ILingoFrameworkGfxNode
    {
        string Text { get; set; }
        int FontSize { get; set; }
        string? Font { get; set; }
        LingoColor FontColor { get; set; }
        int LineHeight { get; set; }
        LingoTextWrapMode WrapMode { get; set; }
        LingoTextAlignment TextAlignment { get; set; }
    }
}
