using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using System.Threading.Tasks;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputText : IAbstFrameworkInputText
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public int MaxLength { get; set; }
    [Parameter] public string? Font { get; set; }
    [Parameter] public int FontSize { get; set; } = 14;
    [Parameter] public bool IsMultiLine { get; set; }
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    private Task HandleInput(ChangeEventArgs e)
    {
        Text = e.Value?.ToString() ?? string.Empty;
        ValueChanged?.Invoke();
        return Task.CompletedTask;
    }
}
