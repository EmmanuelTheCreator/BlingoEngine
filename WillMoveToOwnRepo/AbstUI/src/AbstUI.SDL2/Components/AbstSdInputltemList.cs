using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdInputltemList : AbstSdlComponent, IAbstFrameworkItemList, IDisposable
    {
        public AbstSdInputltemList(AbstSdlComponentFactory factory) : base(factory)
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            ImGui.PushID(Name);

            if (!Enabled)
                ImGui.BeginDisabled();

            if (ImGui.BeginListBox("##list", new Vector2(Width, Height)))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    bool selected = i == SelectedIndex;
                    if (ImGui.Selectable(item.Value, selected))
                    {
                        SelectedIndex = i;
                        SelectedKey = item.Key;
                        SelectedValue = item.Value;
                        ValueChanged?.Invoke();
                    }
                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndListBox();
            }

            if (!Enabled)
                ImGui.EndDisabled();

            ImGui.PopID();

            return AbstSDLRenderResult.RequireRender();
        }
    }
}
