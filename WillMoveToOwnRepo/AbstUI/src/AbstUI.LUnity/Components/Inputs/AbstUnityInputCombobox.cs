using System;
using System.Collections.Generic;
using AbstUI.Components.Inputs;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.LUnity.Primitives;
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
        UpdateColors();
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
    public string? ItemFont { get; set; }
    public int ItemFontSize { get; set; } = 11;
    private AColor _itemTextColor = AbstDefaultColors.InputTextColor;
    public AColor ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; UpdateColors(); } }
    private AColor _itemSelectedTextColor = AbstDefaultColors.InputSelectionText;
    public AColor ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; UpdateColors(); } }
    private AColor _itemSelectedBackgroundColor = AbstDefaultColors.InputAccentColor;
    public AColor ItemSelectedBackgroundColor { get => _itemSelectedBackgroundColor; set { _itemSelectedBackgroundColor = value; UpdateColors(); } }
    private AColor _itemSelectedBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemSelectedBorderColor { get => _itemSelectedBorderColor; set { _itemSelectedBorderColor = value; UpdateColors(); } }
    private AColor _itemHoverTextColor = AbstDefaultColors.InputTextColor;
    public AColor ItemHoverTextColor { get => _itemHoverTextColor; set { _itemHoverTextColor = value; UpdateColors(); } }
    private AColor _itemHoverBackgroundColor = AbstDefaultColors.ListHoverColor;
    public AColor ItemHoverBackgroundColor { get => _itemHoverBackgroundColor; set { _itemHoverBackgroundColor = value; UpdateColors(); } }
    private AColor _itemHoverBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemHoverBorderColor { get => _itemHoverBorderColor; set { _itemHoverBorderColor = value; UpdateColors(); } }
    private AColor _itemPressedTextColor = AbstDefaultColors.InputSelectionText;
    public AColor ItemPressedTextColor { get => _itemPressedTextColor; set { _itemPressedTextColor = value; UpdateColors(); } }
    private AColor _itemPressedBackgroundColor = AbstDefaultColors.InputAccentColor;
    public AColor ItemPressedBackgroundColor { get => _itemPressedBackgroundColor; set { _itemPressedBackgroundColor = value; UpdateColors(); } }
    private AColor _itemPressedBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemPressedBorderColor { get => _itemPressedBorderColor; set { _itemPressedBorderColor = value; UpdateColors(); } }

    private void UpdateColors()
    {
        if (_dropdown.captionText != null)
            _dropdown.captionText.color = _itemTextColor.ToUnityColor();
        var colors = _dropdown.colors;
        colors.normalColor = _itemTextColor.ToUnityColor();
        colors.highlightedColor = _itemHoverBackgroundColor.ToUnityColor();
        colors.pressedColor = _itemPressedBackgroundColor.ToUnityColor();
        colors.selectedColor = _itemSelectedBackgroundColor.ToUnityColor();
        _dropdown.colors = colors;
    }
}
