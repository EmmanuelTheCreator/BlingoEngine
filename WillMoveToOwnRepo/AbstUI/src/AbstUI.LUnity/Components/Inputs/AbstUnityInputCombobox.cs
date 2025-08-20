using System;
using System.Collections.Generic;
using AbstUI.Components.Inputs;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputCombobox"/>.
/// </summary>
internal class AbstUnityInputCombobox : AbstUnityComponent, IAbstFrameworkInputCombobox
{
    private readonly Dropdown _dropdown;
    private readonly List<KeyValuePair<string, string>> _items = new();
    private int _selectedIndex = -1;

    public AbstUnityInputCombobox() : base(CreateGameObject(out var dropdown))
    {
        _dropdown = dropdown;
        _dropdown.onValueChanged.AddListener(OnSelectionChanged);
    }

    private static GameObject CreateGameObject(out Dropdown dropdown)
    {
        var go = new GameObject("Dropdown");
        go.AddComponent<Image>();
        dropdown = go.AddComponent<Dropdown>();
        return go;
    }

    private void OnSelectionChanged(int index)
    {
        if (_selectedIndex == index) return;
        _selectedIndex = index;
        UpdateSelection(index);
    }

    private void UpdateSelection(int index)
    {
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

    public bool Enabled
    {
        get => _dropdown.interactable;
        set => _dropdown.interactable = value;
    }

    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            _dropdown.value = value;
            UpdateSelection(value);
        }
    }

    public string? SelectedKey
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex].Key : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }
            var index = _items.FindIndex(it => it.Key == value);
            if (index >= 0)
                SelectedIndex = index;
        }
    }

    public string? SelectedValue
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex].Value : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }
            var index = _items.FindIndex(it => it.Value == value);
            if (index >= 0)
                SelectedIndex = index;
        }
    }

    public event Action? ValueChanged;

    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
        _dropdown.options.Add(new Dropdown.OptionData { text = value });
    }

    public void ClearItems()
    {
        _items.Clear();
        _dropdown.options.Clear();
        SelectedIndex = -1;
    }
}
