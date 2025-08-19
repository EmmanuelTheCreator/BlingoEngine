using AbstUI.Primitives;

namespace AbstUI.Styles;

/// <summary>
/// Style for <see cref="AbstUI.Components.AbstLabel"/>.
/// </summary>
public class AbstLabelStyle : AbstComponentStyle
{
    public string? Font { get; set; }
    public int? FontSize { get; set; }
    public AColor? FontColor { get; set; }
}
