using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorLabel : IAbstFrameworkLabel
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public int FontSize { get; set; } = 14;
    [Parameter] public string? Font { get; set; }
    [Parameter] public AColor FontColor { get; set; } = AColor.FromRGB(0, 0, 0);
    [Parameter] public int LineHeight { get; set; }
    [Parameter] public ATextWrapMode WrapMode { get; set; }
    [Parameter] public AbstTextAlignment TextAlignment { get; set; } = AbstTextAlignment.Left;
}
