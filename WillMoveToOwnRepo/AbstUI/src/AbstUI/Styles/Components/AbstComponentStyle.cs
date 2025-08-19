using AbstUI.Primitives;

namespace AbstUI.Styles;

/// <summary>
/// Base style applicable to all components.
/// </summary>
public class AbstComponentStyle
{
    public bool? Visibility { get; set; }
    public AMargin? Margin { get; set; }
    public float? Width { get; set; }
    public float? Height { get; set; }
}
