using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    public class AbstImGuiTabContainer : AbstImGuiComponent, IAbstFrameworkTabContainer, IDisposable
    {
        private readonly List<IAbstFrameworkTabItem> _children = new();
        private int _selectedIndex = -1;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstImGuiTabContainer(AbstImGuiComponentFactory factory) : base(factory)
        {
        }

        public string SelectedTabName =>
            _selectedIndex >= 0 && _selectedIndex < _children.Count ? _children[_selectedIndex].Title : string.Empty;

        public void AddTab(IAbstFrameworkTabItem content)
        {
            _children.Add(content);
            if (_selectedIndex == -1)
                _selectedIndex = 0;
        }

        public void RemoveTab(IAbstFrameworkTabItem content)
        {
            var index = _children.IndexOf(content);
            if (index >= 0)
            {
                _children.RemoveAt(index);
                if (_selectedIndex >= _children.Count)
                    _selectedIndex = _children.Count - 1;
            }
        }

        public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _children.ToArray();

        public void ClearTabs()
        {
            _children.Clear();
            _selectedIndex = -1;
        }

        public void SelectTabByName(string tabName)
        {
            var idx = _children.FindIndex(t => t.Title == tabName);
            if (idx >= 0)
                _selectedIndex = idx;
        }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            global::ImGuiNET.ImGui.BeginChild("##tabs", new Vector2(Width, Height), ImGuiChildFlags.None);

            if (global::ImGuiNET.ImGui.BeginTabBar("##tabbar"))
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    var tab = _children[i];
                    if (global::ImGuiNET.ImGui.BeginTabItem(tab.Title))
                    {
                        _selectedIndex = i;
                        if (tab.Content?.FrameworkObj is AbstImGuiComponent comp)
                        {
                            var origin = global::ImGuiNET.ImGui.GetCursorScreenPos();
                            var childCtx = context.CreateNew(origin);
                            comp.Render(childCtx);
                        }
                        global::ImGuiNET.ImGui.EndTabItem();
                    }
                }
                global::ImGuiNET.ImGui.EndTabBar();
            }

            global::ImGuiNET.ImGui.EndChild();
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            ClearTabs();
            base.Dispose();
        }
    }

    public class AbstImGuiTabItem : AbstImGuiComponent, IAbstFrameworkTabItem
    {
        public AbstImGuiTabItem(AbstImGuiComponentFactory factory, AbstTabItem tab) : base(factory)
        {
            tab.Init(this);
        }

        public string Title { get; set; } = string.Empty;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public IAbstNode? Content { get; set; }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public override void Dispose() => base.Dispose();

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context) => AbstImGuiRenderResult.RequireRender();
    }
}
