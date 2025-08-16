using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiInputltemList : AbstImGuiComponent, IAbstFrameworkItemList, IDisposable
    {
        public AbstImGuiInputltemList(AbstImGuiComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

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
        public override void Dispose() => base.Dispose();

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);

            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();

            if (global::ImGuiNET.ImGui.BeginListBox("##list", new Vector2(Width, Height)))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    bool selected = i == SelectedIndex;
                    if (global::ImGuiNET.ImGui.Selectable(item.Value, selected))
                    {
                        SelectedIndex = i;
                        SelectedKey = item.Key;
                        SelectedValue = item.Value;
                        ValueChanged?.Invoke();
                    }
                    if (selected)
                        global::ImGuiNET.ImGui.SetItemDefaultFocus();
                }
                global::ImGuiNET.ImGui.EndListBox();
            }

            if (!Enabled)
                global::ImGuiNET.ImGui.EndDisabled();

            global::ImGuiNET.ImGui.PopID();

            return AbstImGuiRenderResult.RequireRender();
        }
    }
}
