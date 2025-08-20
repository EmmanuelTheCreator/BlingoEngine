using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorColorPicker : IAbstFrameworkColorPicker
{
    [Parameter] public AColor Color { get; set; } = AColor.FromRGB(0, 0, 0);
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    private void HandleInput(ChangeEventArgs e)
    {
        if (e.Value is string hex && hex.StartsWith("#"))
        {
            Color = AColor.FromHex(hex);
            ValueChanged?.Invoke();
        }
    }
}
