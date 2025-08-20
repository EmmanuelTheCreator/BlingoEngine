using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputCheckbox
{
    private AbstBlazorInputCheckboxComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorInputCheckboxComponent Component { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _component = Component;
        SyncFromComponent();
        _component.Changed += OnComponentChanged;
    }

    private void OnComponentChanged()
    {
        SyncFromComponent();
        RequestRender();
    }

    private void SyncFromComponent()
    {
        Visibility = _component.Visibility;
        Width = _component.Width;
        Height = _component.Height;
        Margin = _component.Margin;
    }

    private void OnChange(ChangeEventArgs e)
    {
        _component.Checked = e.Value switch
        {
            bool b => b,
            string s => s == "on" || s == "true",
            _ => false
        };
        _component.RaiseValueChanged();
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
