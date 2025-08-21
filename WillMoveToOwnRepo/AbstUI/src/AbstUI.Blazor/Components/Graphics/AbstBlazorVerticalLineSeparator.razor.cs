using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components.Graphics;

public partial class AbstBlazorVerticalLineSeparator
{
    private AbstBlazorVerticalLineSeparatorComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorVerticalLineSeparatorComponent Component { get; set; } = default!;

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

    private string BuildDivStyle()
    {
        var style = BuildStyle();
        style += "border-left:1px solid #ffffff;border-right:1px solid #a0a0a0;width:0;";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
