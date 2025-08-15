using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific label used for displaying text.
    /// </summary>
    public interface IAbstUIFrameworkGfxLabel : IAbstUIFrameworkGfxNode
    {
        string Text { get; set; }
        int FontSize { get; set; }
        string? Font { get; set; }
        AColor FontColor { get; set; }
        int LineHeight { get; set; }
        ATextWrapMode WrapMode { get; set; }
        AbstTextAlignment TextAlignment { get; set; }
    }
}
