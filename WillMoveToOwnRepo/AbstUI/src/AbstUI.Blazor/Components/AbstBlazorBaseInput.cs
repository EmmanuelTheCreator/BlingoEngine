using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components;

public abstract class AbstBlazorBaseInput : IAbstFrameworkNodeInput
{
    private bool _isdisposed;
   [Parameter]  public bool Enabled {get;set; }
   [Parameter]  public float X {get;set; }
   [Parameter]  public float Y {get;set; }
    [Parameter] public string Name { get; set; } = "";
   [Parameter]  public bool Visibility {get;set; }
   [Parameter]  public float Width {get;set; }
   [Parameter]  public float Height {get;set; }
    [Parameter] public AMargin Margin {get;set; }

    public abstract object FrameworkNode { get; }

    public event Action? ValueChanged;


    protected void ValueChangedInvoke()
    {
        if (_isdisposed) return;
        ValueChanged?.Invoke();
    }
    public void Dispose()
    {
        if (_isdisposed) return;
        _isdisposed = true;
        OnDispose();
    }
    protected void OnDispose() { }
}
