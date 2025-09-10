using Microsoft.AspNetCore.Components;

namespace AbstUI.Blazor.Components.Containers;

public partial class AbstBlazorZoomBox
{
    private AbstBlazorZoomBoxComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorZoomBoxComponent Component { get; set; } = default!;

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

    private string BuildZoomBoxStyle()
    {
        var style = $"position:absolute;left:{_component.X}px;top:{_component.Y}px;overflow:hidden;";
        return $"{BuildStyle()}{style}";
    }

    private string BuildContentStyle()
    {
        var content = _component.Content;
        var x = content?.X ?? 0f;
        var y = content?.Y ?? 0f;
        return $"position:absolute;left:{x}px;top:{y}px;transform-origin:top left;transform:scale({_component.ScaleH},{_component.ScaleV});";
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
