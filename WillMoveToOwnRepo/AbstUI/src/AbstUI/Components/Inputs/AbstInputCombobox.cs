using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Engine level wrapper for a combobox input.
    /// </summary>
    public class AbstInputCombobox : AbstInputBase<IAbstFrameworkInputCombobox>, IAbstSdlHasSelectableCollectionStyle, IHasTextBackgroundBorderColor
    {
        public IReadOnlyList<KeyValuePair<string, string>> Items => _framework.Items;
        public void AddItem(string key, string value) => _framework.AddItem(key, value);
        public void ClearItems() => _framework.ClearItems();
        public int SelectedIndex { get => _framework.SelectedIndex; set => _framework.SelectedIndex = value; }
        public string? SelectedKey { get => _framework.SelectedKey; set => _framework.SelectedKey = value; }
        public string? SelectedValue { get => _framework.SelectedValue; set => _framework.SelectedValue = value; }

        public string? ItemFont { get => _framework.ItemFont; set => _framework.ItemFont = value; }
        public int ItemFontSize { get => _framework.ItemFontSize; set => _framework.ItemFontSize = value; }
        public AColor ItemTextColor { get => _framework.ItemTextColor; set => _framework.ItemTextColor = value; }
        public AColor ItemSelectedTextColor { get => _framework.ItemSelectedTextColor; set => _framework.ItemSelectedTextColor = value; }
        public AColor ItemSelectedBackgroundColor { get => _framework.ItemSelectedBackgroundColor; set => _framework.ItemSelectedBackgroundColor = value; }
        public AColor ItemSelectedBorderColor { get => _framework.ItemSelectedBorderColor; set => _framework.ItemSelectedBorderColor = value; }
        public AColor ItemHoverTextColor { get => _framework.ItemHoverTextColor; set => _framework.ItemHoverTextColor = value; }
        public AColor ItemHoverBackgroundColor { get => _framework.ItemHoverBackgroundColor; set => _framework.ItemHoverBackgroundColor = value; }
        public AColor ItemHoverBorderColor { get => _framework.ItemHoverBorderColor; set => _framework.ItemHoverBorderColor = value; }
        public AColor ItemPressedTextColor { get => _framework.ItemPressedTextColor; set => _framework.ItemPressedTextColor = value; }
        public AColor ItemPressedBackgroundColor { get => _framework.ItemPressedBackgroundColor; set => _framework.ItemPressedBackgroundColor = value; }
        public AColor ItemPressedBorderColor { get => _framework.ItemPressedBorderColor; set => _framework.ItemPressedBorderColor = value; }

        public AColor TextColor { get => ((IHasTextBackgroundBorderColor)_framework).TextColor; set => ((IHasTextBackgroundBorderColor)_framework).TextColor = value; }
        public AColor BackgroundColor { get => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor; set => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor = value; }
        public AColor BorderColor { get => ((IHasTextBackgroundBorderColor)_framework).BorderColor; set => ((IHasTextBackgroundBorderColor)_framework).BorderColor = value; }
    }
}
