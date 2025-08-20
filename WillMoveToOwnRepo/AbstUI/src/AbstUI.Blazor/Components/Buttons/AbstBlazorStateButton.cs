using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorStateButton : IAbstFrameworkStateButton
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public bool IsOn { get; set; }
    [Parameter] public bool Enabled { get; set; } = true;
    public IAbstTexture2D? TextureOn { get; set; }
    public IAbstTexture2D? TextureOff { get; set; }

    public event Action? ValueChanged;

    private void HandleClick()
    {
        if (!Enabled) return;
        IsOn = !IsOn;
        ValueChanged?.Invoke();
    }
}
