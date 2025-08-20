using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorColorPicker : AbstBlazorBaseInput, IAbstFrameworkColorPicker
{
    [Parameter] public AColor Color { get; set; } = AColor.FromRGB(0, 0, 0);
    [Parameter] public bool Enabled { get; set; } = true;


    private void HandleInput(ChangeEventArgs e)
    {
        if (e.Value is string hex && hex.StartsWith("#"))
        {
            Color = AColor.FromHex(hex);
            ValueChangedInvoke();
        }
    }
}
