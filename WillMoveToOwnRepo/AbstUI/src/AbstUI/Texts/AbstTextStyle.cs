using AbstUI.Primitives;

namespace AbstUI.Texts;

/// <summary>
/// Represents a collection of style properties that can be applied to text.
/// </summary>
public class AbstTextStyle : IAbstTextStyle
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

    /// <summary>Creates a deep copy of this style.</summary>
    public AbstTextStyle Clone()
        => new()
        {
            Name = Name,
            FontSize = FontSize,
            Font = Font,
            Color = Color,
            Alignment = Alignment,
            Bold = Bold,
            Italic = Italic,
            Underline = Underline,
            LineHeight = LineHeight,
            MarginLeft = MarginLeft,
            MarginRight = MarginRight
        };

    /// <summary>Copies all style properties from another style instance.</summary>
    public void CopyFrom(IAbstTextStyle style)
    {
        Name = style.Name;
        FontSize = style.FontSize;
        Font = style.Font;
        Color = style.Color;
        Alignment = style.Alignment;
        Bold = style.Bold;
        Italic = style.Italic;
        Underline = style.Underline;
        LineHeight = style.LineHeight;
        MarginLeft = style.MarginLeft;
        MarginRight = style.MarginRight;
    }
}
