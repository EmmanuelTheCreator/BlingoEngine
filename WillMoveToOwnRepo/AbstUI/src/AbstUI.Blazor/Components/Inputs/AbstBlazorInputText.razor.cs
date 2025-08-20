using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputText
{
    private AbstBlazorInputTextComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorInputTextComponent Component { get; set; } = default!;

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

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"color:{_component.TextColor.ToHex()};";
        return style;
    }

    private Task HandleInput(ChangeEventArgs e)
    {
        _component.Text = e.Value?.ToString() ?? string.Empty;
        _component.RaiseValueChanged();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
