using System;
using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;
using AbstUI.Blazor;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorItemList
{
    private AbstBlazorItemListComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorItemListComponent Component { get; set; } = default!;

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

    private void HandleChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var index))
        {
            _component.SelectedIndex = index;
            if (index >= 0 && index < _component.Items.Count)
            {
                _component.SelectedKey = _component.Items[index].Key;
                _component.SelectedValue = _component.Items[index].Value;
            }
            else
            {
                _component.SelectedKey = null;
                _component.SelectedValue = null;
            }
            _component.RaiseValueChanged();
        }
    }

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        if (!string.IsNullOrEmpty(_component.ItemFont))
            style += $"font-family:{_component.ItemFont};";
        if (_component.ItemFontSize > 0)
            style += $"font-size:{_component.ItemFontSize}px;";
        style += $"color:{_component.ItemTextColor.ToHex()};";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
