using System.Collections.Generic;

namespace AbstUI.Texts;

public class AbstMarkdownData
{
    public string Markdown { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public List<AbstMDSegment> Segments { get; set; } = new();
    public Dictionary<string, AbstTextStyle> Styles { get; set; } = new();
}
