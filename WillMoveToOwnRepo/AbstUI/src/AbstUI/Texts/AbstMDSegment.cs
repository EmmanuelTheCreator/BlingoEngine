using AbstUI.Primitives;

namespace AbstUI.Texts;

public class AbstMDSegment
{
    public string? FontName { get; set; } = "";
    public int Size { get; set; }
    public AColor? Color { get; set; }
    public string Text { get; set; } = "";
    public AbstTextAlignment Alignment { get; set; } = AbstTextAlignment.Left;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public int MarginLeft { get; set; }
    public int MarginRight { get; set; }
    public int StyleId { get; set; } = -1;
    public bool IsParagraph { get; set; }
}
