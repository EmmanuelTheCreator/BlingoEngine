using AbstUI.Blazor.Components.Texts;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Blazor.Texts;

/// <summary>
/// Helper extensions for working with <see cref="AbstBlazorLabelComponent"/>.
/// </summary>
public static class AbstBlazorLabelExtensions
{
    public static AbstBlazorLabelComponent SetAbstFont(this AbstBlazorLabelComponent label, IAbstFontManager fontManager, string fontName)
    {
        if (fontManager.Get<object>(fontName) != null)
            label.Font = fontName;
        return label;
    }

    public static AbstBlazorLabelComponent SetAbstFontSize(this AbstBlazorLabelComponent label, int fontSize)
    {
        if (fontSize > 0)
            label.FontSize = fontSize;
        return label;
    }

    public static AbstBlazorLabelComponent SetAbstColor(this AbstBlazorLabelComponent label, AColor color)
    {
        label.FontColor = color;
        return label;
    }

    public static string ToCss(this AbstTextAlignment alignment) => alignment switch
    {
        AbstTextAlignment.Center => "center",
        AbstTextAlignment.Right => "right",
        AbstTextAlignment.Justified => "justify",
        _ => "left"
    };

    public static AbstTextAlignment ToAbst(this string alignment) => alignment?.ToLowerInvariant() switch
    {
        "center" => AbstTextAlignment.Center,
        "right" => AbstTextAlignment.Right,
        "justify" => AbstTextAlignment.Justified,
        _ => AbstTextAlignment.Left
    };

    public static AColor GetAbstColor(this AbstBlazorLabelComponent label)
        => label.FontColor;
}
