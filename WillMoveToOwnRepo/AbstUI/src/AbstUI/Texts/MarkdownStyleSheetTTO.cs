
namespace AbstUI.Texts;
#if NET48
using Newtonsoft.Json;
public class MarkdownStyleSheetTTO
{
    [JsonProperty("font-family")]
    public string? FontFamily { get; set; }

    [JsonProperty("font-size")]
    public int? FontSize { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("text-align")]
    public string? TextAlign { get; set; }

    [JsonProperty("font-weight")]
    public string? FontWeight { get; set; }

    [JsonProperty("font-style")]
    public string? FontStyle { get; set; }

    [JsonProperty("text-decoration")]
    public string? TextDecoration { get; set; }

    [JsonProperty("line-height")]
    public int? LineHeight { get; set; }

    [JsonProperty("margin-left")]
    public int? MarginLeft { get; set; }

    [JsonProperty("margin-right")]
    public int? MarginRight { get; set; }

    [JsonProperty("letter-spacing")]
    public int? LetterSpacing { get; set; }
}
#else
using System.Text.Json.Serialization;
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

    [JsonPropertyName("letter-spacing")]
    public int? LetterSpacing { get; set; }
}
#endif

