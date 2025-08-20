using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorColorPicker : IAbstFrameworkColorPicker
{
    [Parameter] public AColor Color { get; set; } = AColor.FromRGB(0, 0, 0);
    private void HandleInput(ChangeEventArgs e)
    {
        if (e.Value is string hex && hex.StartsWith("#"))
        {
            Color = AColor.FromHex(hex);
            ValueChangedInvoke();
        }
    }
}
