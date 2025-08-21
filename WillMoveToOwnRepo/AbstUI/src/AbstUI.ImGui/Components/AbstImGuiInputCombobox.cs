using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.Styles;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiInputCombobox : AbstImGuiComponent, IAbstFrameworkInputCombobox, IDisposable
    {
        public AbstImGuiInputCombobox(AbstImGuiComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

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

        public event Action? ValueChanged;
        public object FrameworkNode => this;
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

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();

            string preview = SelectedValue ?? string.Empty;
            if (global::ImGuiNET.ImGui.BeginCombo("##combo", preview))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    bool selected = i == SelectedIndex;
                    var it = _items[i];
                    if (global::ImGuiNET.ImGui.Selectable(it.Value, selected))
                    {
                        SelectedIndex = i;
                        SelectedKey = it.Key;
                        SelectedValue = it.Value;
                        ValueChanged?.Invoke();
                    }
                    if (selected)
                        global::ImGuiNET.ImGui.SetItemDefaultFocus();
                }
                global::ImGuiNET.ImGui.EndCombo();
            }

            if (!Enabled)
                global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();
    }
}
