using System.Collections.Generic;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Framework specific combo box input.
    /// </summary>
    public interface IAbstFrameworkInputCombobox : IAbstFrameworkNodeInput, IAbstSdlHasSeletectableCollectionStyle
    {
        IReadOnlyList<KeyValuePair<string, string>> Items { get; }
        void AddItem(string key, string value);
        void ClearItems();
        int SelectedIndex { get; set; }
        string? SelectedKey { get; set; }
        string? SelectedValue { get; set; }
    }
}
