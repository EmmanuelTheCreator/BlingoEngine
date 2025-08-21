using Microsoft.AspNetCore.Components;
using System.Numerics;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputNumber<TValue>
    where TValue : INumber<TValue>
{
    private AbstBlazorInputNumberComponent<TValue> _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorInputNumberComponent<TValue> Component { get; set; } = default!;

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
        Enabled = _component.Enabled;
    }

    private void HandleInput(ChangeEventArgs e)
    {
        if (TValue.TryParse(e.Value?.ToString(), null, out var result))
        {
            _component.Value = result;
            _component.RaiseValueChanged();
        }
    }

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"color:{_component.TextColor.ToHex()};";
        style += $"background-color:{_component.BackgroundColor.ToHex()};";
        style += $"border-color:{_component.BorderColor.ToHex()};";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
