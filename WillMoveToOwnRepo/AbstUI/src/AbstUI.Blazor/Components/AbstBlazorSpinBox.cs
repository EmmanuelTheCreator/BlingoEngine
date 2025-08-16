using Microsoft.AspNetCore.Components;
using AbstUI.Components;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorSpinBox : IAbstFrameworkSpinBox
{
    [Parameter] public float Value { get; set; }
    [Parameter] public float Min { get; set; }
    [Parameter] public float Max { get; set; }
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    private void HandleInput(ChangeEventArgs e)
    {
        if (float.TryParse(e.Value?.ToString(), out var v))
        {
            Value = v;
            ValueChanged?.Invoke();
        }
    }
}
