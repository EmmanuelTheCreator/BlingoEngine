using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components.Containers;

public partial class AbstBlazorScrollContainer
{
    private AbstBlazorScrollContainerComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorScrollContainerComponent Component { get; set; } = default!;

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

    private string BuildScrollStyle()
    {
        var style = $"position:absolute;left:{_component.X}px;top:{_component.Y}px;";
        style += _component.ClipContents ? "overflow:hidden;" : "overflow:auto;";
        return $"{BuildStyle()}{style}";
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
