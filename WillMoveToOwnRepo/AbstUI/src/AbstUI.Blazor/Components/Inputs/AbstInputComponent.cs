using Microsoft.AspNetCore.Components;
using AbstUI.Inputs;
using AbstUI.Blazor.Inputs;

namespace AbstUI.Blazor.Components;

/// <summary>
/// Component that routes DOM mouse and keyboard events to the AbstUI input wrappers.
/// </summary>
public partial class AbstInputComponent : AbstBlazorComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Inject] private BlazorMouse<AbstMouseEvent> Mouse { get; set; } = default!;
    [Inject] private BlazorKey Key { get; set; } = default!;
}

