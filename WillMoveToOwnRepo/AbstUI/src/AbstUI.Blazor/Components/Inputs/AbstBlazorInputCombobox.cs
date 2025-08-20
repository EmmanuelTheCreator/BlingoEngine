using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorInputCombobox : IAbstFrameworkInputCombobox
{
    [Parameter] public bool Enabled { get; set; } = true;

    private readonly List<KeyValuePair<string, string>> _items = new();
    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public int SelectedIndex { get; set; } = -1;
    public string? SelectedKey { get; set; }
    public string? SelectedValue { get; set; }

    public event Action? ValueChanged;

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
            ValueChanged?.Invoke();
        }
    }
}
