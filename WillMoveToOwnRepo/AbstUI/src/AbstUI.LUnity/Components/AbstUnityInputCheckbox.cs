using System;
using AbstUI.Components;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputCheckbox"/>.
/// </summary>
internal class AbstUnityInputCheckbox : AbstUnityComponent, IAbstFrameworkInputCheckbox
{
    public bool Enabled { get; set; } = true;

    private bool _checked;
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            ValueChanged?.Invoke();
        }
    }

    public event Action? ValueChanged;
}
