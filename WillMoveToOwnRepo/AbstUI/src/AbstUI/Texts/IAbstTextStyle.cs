using AbstUI.Primitives;

namespace AbstUI.Texts;

/// <summary>
/// Describes styling properties used by <see cref="AbstMarkdownRenderer"/> and related components.
/// </summary>
public interface IAbstTextStyle
{
    string Name { get; set; }
    int FontSize { get; set; }
    string Font { get; set; }
    AColor Color { get; set; }
    AbstTextAlignment Alignment { get; set; }
    bool Bold { get; set; }
    bool Italic { get; set; }
    bool Underline { get; set; }
    int LineHeight { get; set; }
    int MarginLeft { get; set; }
    int MarginRight { get; set; }
    int LetterSpacing { get; set; }
}
