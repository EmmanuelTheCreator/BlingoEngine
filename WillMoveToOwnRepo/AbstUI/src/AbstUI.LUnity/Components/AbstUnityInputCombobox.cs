using System;
using System.Collections.Generic;
using AbstUI.Components;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputCombobox"/>.
/// </summary>
internal class AbstUnityInputCombobox : AbstUnityComponent, IAbstFrameworkInputCombobox
{
    public bool Enabled { get; set; } = true;
    private readonly List<KeyValuePair<string, string>> _items = new();
    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public int SelectedIndex { get; set; } = -1;
    public string? SelectedKey { get; set; }
    public string? SelectedValue { get; set; }

    public event Action? ValueChanged;

    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
    }

    public void ClearItems()
    {
        _items.Clear();
        SelectedIndex = -1;
        SelectedKey = null;
        SelectedValue = null;
    }
}
