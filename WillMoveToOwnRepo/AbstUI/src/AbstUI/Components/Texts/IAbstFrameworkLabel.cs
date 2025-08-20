using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Components.Texts
{
    /// <summary>
    /// Framework specific label used for displaying text.
    /// </summary>
    public interface IAbstFrameworkLabel : IAbstFrameworkNode
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
