using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorWrapPanel : AbstBlazorComponentBase
{
    private AbstBlazorWrapPanelComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorWrapPanelComponent Component { get; set; } = default!;

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
        Name = _component.Name;
    }

    private string BuildWrapStyle()
        => $"{BuildStyle()}display:flex;flex-wrap:wrap;flex-direction:{(_component.Orientation == AOrientation.Horizontal ? "row" : "column")};";

    private string BuildItemStyle()
        => $"margin:{_component.ItemMargin.Y}px {_component.ItemMargin.X}px;";

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
