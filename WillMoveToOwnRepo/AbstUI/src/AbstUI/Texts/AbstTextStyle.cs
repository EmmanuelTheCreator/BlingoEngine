using AbstUI.Primitives;

namespace AbstUI.Texts;

/// <summary>
/// Represents a collection of style properties that can be applied to text.
/// </summary>
public class AbstTextStyle
{
    public string Name { get; set; } = string.Empty;
    public int FontSize { get; set; }
    public string Font { get; set; } = string.Empty;
    public AColor Color { get; set; } = AColors.Black;
    public AbstTextAlignment Alignment { get; set; } = AbstTextAlignment.Left;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public int LineHeight { get; set; }
    public int MarginLeft { get; set; }
    public int MarginRight { get; set; }
}
