using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorPanel : AbstBlazorComponentBase
{
    private AbstBlazorPanelComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorPanelComponent Component { get; set; } = default!;

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
        X = _component.X;
        Y = _component.Y;
        Name = _component.Name;
    }

    private string BuildPanelStyle()
    {
        var style = string.Empty;
        if (_component.BackgroundColor.HasValue)
            style += $"background-color:rgba({_component.BackgroundColor.Value.R},{_component.BackgroundColor.Value.G},{_component.BackgroundColor.Value.B},{_component.BackgroundColor.Value.A / 255f});";
        if (_component.BorderColor.HasValue && _component.BorderWidth > 0)
            style += $"border:{_component.BorderWidth}px solid rgba({_component.BorderColor.Value.R},{_component.BorderColor.Value.G},{_component.BorderColor.Value.B},{_component.BorderColor.Value.A / 255f});";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
