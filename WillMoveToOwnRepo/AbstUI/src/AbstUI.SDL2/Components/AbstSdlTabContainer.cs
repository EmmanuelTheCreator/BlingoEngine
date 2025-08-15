using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    public class AbstSdlTabContainer : AbstSdlComponent, IAbstFrameworkTabContainer, IDisposable
    {
        private readonly List<IAbstFrameworkTabItem> _children = new();
        private int _selectedIndex = -1;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstSdlTabContainer(AbstSdlComponentFactory factory) : base(factory)
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            ImGui.PushID(Name);
            ImGui.BeginChild("##tabs", new Vector2(Width, Height), ImGuiChildFlags.None);

            if (ImGui.BeginTabBar("##tabbar"))
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    var tab = _children[i];
                    if (ImGui.BeginTabItem(tab.Title))
                    {
                        _selectedIndex = i;
                        if (tab.Content?.FrameworkObj is AbstSdlComponent comp)
                        {
                            var origin = ImGui.GetCursorScreenPos();
                            var childCtx = context.CreateNew(origin);
                            comp.Render(childCtx);
                        }
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }

            ImGui.EndChild();
            ImGui.PopID();
            return AbstSDLRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            ClearTabs();
            base.Dispose();
        }
    }

    public class AbstSdlTabItem : AbstSdlComponent, IAbstFrameworkTabItem
    {
        public AbstSdlTabItem(AbstSdlComponentFactory factory, AbstTabItem tab) : base(factory)
        {
            tab.Init(this);
        }

        public string Title { get; set; } = string.Empty;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public IAbstNode? Content { get; set; }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public override void Dispose() => base.Dispose();

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context) => AbstSDLRenderResult.RequireRender();
    }
}
