using System.Text.Json.Serialization;

namespace AbstUI.Texts;

public class MarkdownStyleSheetTTO
{
    [JsonPropertyName("font-family")]
    public string? FontFamily { get; set; }

    [JsonPropertyName("font-size")]
    public int? FontSize { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("text-align")]
    public string? TextAlign { get; set; }

    [JsonPropertyName("font-weight")]
    public string? FontWeight { get; set; }

    [JsonPropertyName("font-style")]
    public string? FontStyle { get; set; }

    [JsonPropertyName("text-decoration")]
    public string? TextDecoration { get; set; }

    [JsonPropertyName("line-height")]
    public int? LineHeight { get; set; }

    [JsonPropertyName("margin-left")]
    public int? MarginLeft { get; set; }

    [JsonPropertyName("margin-right")]
    public int? MarginRight { get; set; }
}
