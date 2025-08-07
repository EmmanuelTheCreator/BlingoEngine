using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    public class SdlGfxTabContainer : ILingoFrameworkGfxTabContainer, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;
        private readonly List<ILingoFrameworkGfxTabItem> _children = new();
        private int _selectedIndex = -1;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public object FrameworkNode => this;

        public SdlGfxTabContainer(nint renderer)
        {
            _renderer = renderer;
        }

        public string SelectedTabName =>
            _selectedIndex >= 0 && _selectedIndex < _children.Count ? _children[_selectedIndex].Title : string.Empty;

        public void AddTab(ILingoFrameworkGfxTabItem content)
        {
            _children.Add(content);
            if (_selectedIndex == -1)
                _selectedIndex = 0;
        }

        public void RemoveTab(ILingoFrameworkGfxTabItem content)
        {
            var index = _children.IndexOf(content);
            if (index >= 0)
            {
                _children.RemoveAt(index);
                if (_selectedIndex >= _children.Count)
                    _selectedIndex = _children.Count - 1;
            }
        }

        public IEnumerable<ILingoFrameworkGfxTabItem> GetTabs() => _children.ToArray();

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

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            ImGui.BeginChild("##tabs", new Vector2(Width, Height), false);

            if (ImGui.BeginTabBar("##tabbar"))
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    var tab = _children[i];
                    if (ImGui.BeginTabItem(tab.Title, _selectedIndex == i))
                    {
                        _selectedIndex = i;
                        if (tab.Content?.FrameworkObj is ISdlRenderElement renderable)
                            renderable.Render();
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }

            ImGui.EndChild();
            ImGui.PopID();
        }

        public void Dispose()
        {
            ClearTabs();
        }
    }

    public partial class SdlGfxTabItem : ILingoFrameworkGfxTabItem
    {
        private readonly nint _renderer;

        public SdlGfxTabItem(nint renderer, LingoGfxTabItem tab)
        {
            _renderer = renderer;
            tab.Init(this);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public ILingoGfxNode? Content { get; set; }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public void Dispose()
        {
        }
    }
}
