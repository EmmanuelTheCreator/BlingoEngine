using Microsoft.AspNetCore.Components;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorSpinBox : IAbstFrameworkSpinBox, IHasTextBackgroundBorderColor
{
    [Parameter] public float Value { get; set; }
    [Parameter] public float Min { get; set; }
    [Parameter] public float Max { get; set; }
    [Parameter] public AColor TextColor { get; set; } = AbstDefaultColors.InputTextColor;
    [Parameter] public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
    [Parameter] public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;


    private void HandleInput(ChangeEventArgs e)
    {
        if (float.TryParse(e.Value?.ToString(), out var v))
        {
            Value = v;
            ValueChangedInvoke();
        }
    }

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"color:{TextColor.ToHex()};";
        style += $"background-color:{BackgroundColor.ToHex()};";
        style += $"border-color:{BorderColor.ToHex()};";
        return style;
    }
}
