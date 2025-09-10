using Microsoft.AspNetCore.Components;
using AbstUI.Inputs;
using AbstUI.Blazor.Inputs;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Components.Inputs;

/// <summary>
/// Component that routes DOM mouse and keyboard events to the AbstUI input wrappers.
/// </summary>
public partial class AbstInputComponent
{
    private GlobalBlazorMouse _mouse;
    private GlobalBlazorKey _key;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Inject] private IJSRuntime _js{ get; set; } = default!;
    private AbstBlazorGlobalMouse<GlobalBlazorMouse, AbstMouseEvent> Mouse { get; set; } = default!;
    private BlazorKey Key { get; set; } = default!;

    protected override void OnInitialized()
    {
        _mouse = new GlobalBlazorMouse(_js);
        _key = new GlobalBlazorKey();
        Mouse = _mouse.Framework<AbstBlazorGlobalMouse<GlobalBlazorMouse, AbstMouseEvent>>();
        Key = _key.Framework<BlazorKey>();
        base.OnInitialized();
    }
}

