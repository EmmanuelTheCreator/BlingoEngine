using AbstUI.Primitives;

namespace AbstUI.Styles;

/// <summary>
/// Style for <see cref="AbstUI.Components.AbstColorPicker"/>.
/// </summary>
public class AbstColorPickerStyle : AbstInputStyle
{
    /// <summary>
    /// Default color shown when no value is set.
    /// </summary>
    public AColor? DefaultColor { get; set; }
}
