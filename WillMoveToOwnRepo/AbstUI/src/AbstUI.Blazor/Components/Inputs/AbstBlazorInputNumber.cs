using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;
using System.Numerics;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputNumber<TValue> : IAbstFrameworkInputNumber<TValue>
    where TValue : INumber<TValue>
{
    [Parameter] public TValue Value { get; set; } = TValue.Zero;
    [Parameter] public TValue Min { get; set; } = TValue.Zero;
    [Parameter] public TValue Max { get; set; } = TValue.Zero;
    [Parameter] public ANumberType NumberType { get; set; } = ANumberType.Float;
    [Parameter] public int FontSize { get; set; } = 14;
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
