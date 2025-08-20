using System.Collections.Generic;
using AbstUI.Components.Buttons;
using AbstUI.Components.Inputs;
using AbstUI.Styles.Components;

namespace AbstUI.Styles;

/// <summary>
/// Helper to register default styles for AbstUI components.
/// </summary>
public static class AbstDefaultStyles
{
    /// <summary>
    /// Registers default styling for common input components.
    /// </summary>
    public static void RegisterInputStyles(IAbstStyleManager styleManager)
    {
        var checkboxStyle = new AbstInputCheckboxStyle();
        var stateButtonStyle = new AbstStateButtonStyle();
        var sliderStyle = new AbstInputSliderStyle();
        var textStyle = new AbstInputTextStyle();
        var numberStyle = new AbstInputNumberStyle();
        var comboboxStyle = new AbstInputComboboxStyle();
        var spinBoxStyle = new AbstInputSpinBoxStyle();

        var styles = new List<AbstInputStyle>
        {
            checkboxStyle,
            stateButtonStyle,
            sliderStyle,
            textStyle,
            numberStyle,
            comboboxStyle,
            spinBoxStyle,
        };

        foreach (var style in styles)
        {
            style.BorderColor = AbstDefaultColors.InputBorderColor;
            style.TextColor = AbstDefaultColors.InputTextColor;
            style.AccentColor = AbstDefaultColors.InputAccentColor;
            style.Font = null;
            style.FontSize = 11;
        }

        styleManager.Register<AbstInputCheckbox>(checkboxStyle);
        styleManager.Register<AbstStateButton>(stateButtonStyle);
        styleManager.Register<AbstInputSlider<int>>(sliderStyle);
        styleManager.Register<AbstInputSlider<float>>(sliderStyle);
        styleManager.Register<AbstInputText>(textStyle);
        styleManager.Register<AbstInputNumber<int>>(numberStyle);
        styleManager.Register<AbstInputNumber<float>>(numberStyle);
        styleManager.Register<AbstInputCombobox>(comboboxStyle);
        styleManager.Register<AbstInputSpinBox>(spinBoxStyle);
    }
}
