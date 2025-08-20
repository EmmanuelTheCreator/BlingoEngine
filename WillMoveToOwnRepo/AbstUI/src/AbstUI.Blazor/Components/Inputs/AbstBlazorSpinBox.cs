using Microsoft.AspNetCore.Components;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorSpinBox : AbstBlazorBaseInput, IAbstFrameworkSpinBox
{
    [Parameter] public float Value { get; set; }
    [Parameter] public float Min { get; set; }
    [Parameter] public float Max { get; set; }


    private void HandleInput(ChangeEventArgs e)
    {
        if (float.TryParse(e.Value?.ToString(), out var v))
        {
            Value = v;
            ValueChangedInvoke();
        }
    }
}
