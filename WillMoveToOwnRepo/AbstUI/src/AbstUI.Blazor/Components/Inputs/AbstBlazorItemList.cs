using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorItemList : IAbstFrameworkItemList
{
    private readonly List<KeyValuePair<string, string>> _items = new();
    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public int SelectedIndex { get; set; } = -1;
    public string? SelectedKey { get; set; }
    public string? SelectedValue { get; set; }

    public string? ItemFont { get; set; }
    public int ItemFontSize { get; set; } = 11;
    public AColor ItemTextColor { get; set; } = AbstDefaultColors.InputTextColor;
    public AColor ItemSelectedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
    public AColor ItemSelectedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
    public AColor ItemSelectedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
    public AColor ItemHoverTextColor { get; set; } = AbstDefaultColors.InputTextColor;
    public AColor ItemHoverBackgroundColor { get; set; } = AbstDefaultColors.ListHoverColor;
    public AColor ItemHoverBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
    public AColor ItemPressedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
    public AColor ItemPressedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
    public AColor ItemPressedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;


    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
        RequestRender();
    }

    public void ClearItems()
    {
        _items.Clear();
        SelectedIndex = -1;
        SelectedKey = null;
        SelectedValue = null;
        RequestRender();
    }

    private void HandleChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var index))
        {
            SelectedIndex = index;
            if (index >= 0 && index < _items.Count)
            {
                SelectedKey = _items[index].Key;
                SelectedValue = _items[index].Value;
            }
            else
            {
                SelectedKey = null;
                SelectedValue = null;
            }
            ValueChangedInvoke();
        }
    }

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        if (!string.IsNullOrEmpty(ItemFont))
            style += $"font-family:{ItemFont};";
        if (ItemFontSize > 0)
            style += $"font-size:{ItemFontSize}px;";
        style += $"color:{ItemTextColor.ToHex()};";
        return style;
    }
}
