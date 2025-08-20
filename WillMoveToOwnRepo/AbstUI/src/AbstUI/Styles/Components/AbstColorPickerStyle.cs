using AbstUI.Primitives;

namespace AbstUI.Styles.Components;

/// <summary>
/// Style for <see cref="AbstUI.Components.Inputs.AbstColorPicker"/>.
/// </summary>
public class AbstColorPickerStyle : AbstInputStyle
{
    /// <summary>
    /// Default color shown when no value is set.
    /// </summary>
    public AColor? DefaultColor { get; set; }
}
