using AbstUI.Primitives;

namespace AbstUI.Styles.Components;

/// <summary>
/// Base style for container components.
/// </summary>
public class AbstContainerStyle : AbstComponentStyle
{
    public AColor? BackgroundColor { get; set; }
    public AColor? BorderColor { get; set; }
    public float? BorderWidth { get; set; }
}
