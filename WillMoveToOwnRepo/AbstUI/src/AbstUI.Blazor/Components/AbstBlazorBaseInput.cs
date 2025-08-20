using Microsoft.AspNetCore.Components;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components;

/// <summary>
/// Base class for all Blazor input components providing common behaviour
/// such as enabled state management and disposal handling.
/// </summary>
public abstract class AbstBlazorBaseInput : AbstBlazorComponentBase, IAbstFrameworkNodeInput
{
    private bool _isDisposed;

    /// <summary>Whether the control is enabled.</summary>
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    protected void ValueChangedInvoke()
    {
        if (_isDisposed) return;
        ValueChanged?.Invoke();
    }

    public override void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        OnDispose();
        base.Dispose();
    }

    /// <summary>Hook for derived classes to dispose additional resources.</summary>
    protected virtual void OnDispose() { }
}
