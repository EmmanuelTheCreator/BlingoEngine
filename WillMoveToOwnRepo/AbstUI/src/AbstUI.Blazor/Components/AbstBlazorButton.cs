using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorButton : IAbstFrameworkButton
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public bool Enabled { get; set; } = true;
    public IAbstTexture2D? IconTexture { get; set; }

    public event Action? Pressed;

    private void HandleClick()
    {
        if (Enabled)
            Pressed?.Invoke();
    }
}
