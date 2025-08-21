using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using System.Numerics;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputNumber<TValue> : IAbstFrameworkInputNumber<TValue>, IHasTextBackgroundBorderColor
    where TValue : INumber<TValue>
{
    [Parameter] public TValue Value { get; set; } = TValue.Zero;
    [Parameter] public TValue Min { get; set; } = TValue.Zero;
    [Parameter] public TValue Max { get; set; } = TValue.Zero;
    [Parameter] public ANumberType NumberType { get; set; } = ANumberType.Float;
    [Parameter] public int FontSize { get; set; } = 14;
    [Parameter] public AColor TextColor { get; set; } = AbstDefaultColors.InputTextColor;
    [Parameter] public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
    [Parameter] public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

    private void HandleInput(ChangeEventArgs e)
    {
        if (TValue.TryParse(e.Value?.ToString(), null, out var result))
        {
            Value = result;
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
