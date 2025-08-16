using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using System.Numerics;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputSlider<TValue> : IAbstFrameworkInputSlider<TValue>
    where TValue : INumber<TValue>
{
    [Parameter] public TValue Value { get; set; } = TValue.Zero;
    [Parameter] public TValue MinValue { get; set; } = TValue.Zero;
    [Parameter] public TValue MaxValue { get; set; } = TValue.One;
    [Parameter] public TValue Step { get; set; } = TValue.One;
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    private void HandleInput(ChangeEventArgs e)
    {
        if (TValue.TryParse(e.Value?.ToString(), null, out var result))
        {
            Value = result;
            ValueChanged?.Invoke();
        }
    }
}
