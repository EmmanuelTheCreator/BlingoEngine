using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Buttons;

namespace AbstUI.Blazor.Components.Buttons;

public partial class AbstBlazorStateButton : IAbstFrameworkStateButton
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public bool IsOn { get; set; }
    public IAbstTexture2D? TextureOn { get; set; }
    public IAbstTexture2D? TextureOff { get; set; }

    private void HandleClick()
    {
        if (!Enabled) return;
        IsOn = !IsOn;
        ValueChangedInvoke();
    }
}
