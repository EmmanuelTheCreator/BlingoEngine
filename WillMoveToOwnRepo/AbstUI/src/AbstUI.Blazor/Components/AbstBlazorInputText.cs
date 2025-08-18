using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using System.Threading.Tasks;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputText : IAbstFrameworkInputText
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public int MaxLength { get; set; }
    [Parameter] public string? Font { get; set; }
    [Parameter] public int FontSize { get; set; } = 14;
    [Parameter] public bool IsMultiLine { get; set; }
    [Parameter] public bool Enabled { get; set; } = true;
    [Parameter] public AColor TextColor { get; set; } = AColors.Black;

    public event Action? ValueChanged;

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"color:{TextColor.ToHex()};";
        return style;
    }

    private Task HandleInput(ChangeEventArgs e)
    {
        Text = e.Value?.ToString() ?? string.Empty;
        ValueChanged?.Invoke();
        return Task.CompletedTask;
    }
}
