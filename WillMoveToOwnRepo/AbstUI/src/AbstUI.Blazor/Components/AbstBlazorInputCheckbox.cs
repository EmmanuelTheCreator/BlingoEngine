using Microsoft.AspNetCore.Components;
using AbstUI.Components;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputCheckbox : IAbstFrameworkInputCheckbox
{
    [Parameter] public bool Checked { get; set; }
    [Parameter] public bool Enabled { get; set; } = true;

    public event Action? ValueChanged;

    private void OnChange(ChangeEventArgs e)
    {
        Checked = e.Value switch
        {
            bool b => b,
            string s => s == "on" || s == "true",
            _ => false
        };
        ValueChanged?.Invoke();
    }
}
