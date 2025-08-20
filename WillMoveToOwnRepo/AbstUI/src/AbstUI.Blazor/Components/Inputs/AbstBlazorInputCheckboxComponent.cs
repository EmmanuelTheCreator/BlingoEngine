using System;
using AbstUI.Components.Inputs;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputCheckboxComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputCheckbox
{
    private bool _checked;
    public bool Checked
    {
        get => _checked;
        set { if (_checked != value) { _checked = value; RaiseChanged(); } }
    }

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; RaiseChanged(); } }
    }

    public event Action? ValueChanged;

    public void RaiseValueChanged() => ValueChanged?.Invoke();
}
