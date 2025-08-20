using AbstUI.Primitives;

namespace AbstUI.Styles.Components;

/// <summary>
/// Base style for input components.
/// </summary>
public class AbstInputStyle : AbstComponentStyle
{
    public string? Font { get; set; }
    public int? FontSize { get; set; }
    public AColor? TextColor { get; set; }
    public AColor? BorderColor { get; set; }
    public AColor? AccentColor { get; set; }
}
