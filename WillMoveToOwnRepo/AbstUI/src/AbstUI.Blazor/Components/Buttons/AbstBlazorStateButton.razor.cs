using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Blazor.Primitives;

namespace AbstUI.Blazor.Components.Buttons;

public partial class AbstBlazorStateButton
{
    private AbstBlazorStateButtonComponent _component = default!;
    private bool _hover;
    private bool _pressed;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorStateButtonComponent Component { get; set; } = default!;

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

    private void HandleClick()
    {
        if (!Enabled) return;
        _component.IsOn = !_component.IsOn;
        _component.RaiseValueChanged();
    }

    private void HandleMouseOver() => _hover = true;
    private void HandleMouseOut() { _hover = false; _pressed = false; }
    private void HandleMouseDown() { if (Enabled) _pressed = true; }
    private void HandleMouseUp() => _pressed = false;

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        var border = _pressed || _component.IsOn ? _component.BorderPressedColor : _hover ? _component.BorderHoverColor : _component.BorderColor;
        var bg = _pressed || _component.IsOn ? _component.BackgroundPressedColor : _hover ? _component.BackgroundHoverColor : _component.BackgroundColor;
        style += $"border:1px solid {border.ToCss()};background:{bg.ToCss()};color:{_component.TextColor.ToCss()};";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
