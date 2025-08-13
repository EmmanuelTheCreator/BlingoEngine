using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    public class SdlGfxTabContainer : SdlGfxComponent, ILingoFrameworkGfxTabContainer, IDisposable
    {
        private readonly List<ILingoFrameworkGfxTabItem> _children = new();
        private int _selectedIndex = -1;

        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public object FrameworkNode => this;

        public SdlGfxTabContainer(SdlGfxFactory factory) : base(factory)
        {
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

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
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
                    bool open = _selectedIndex == i;
                    if (ImGui.BeginTabItem(tab.Title, ref open))
                    {
                        _selectedIndex = i;
                        if (tab.Content?.FrameworkObj is SdlGfxComponent comp)
                        {
                            var origin = ImGui.GetCursorScreenPos();
                            var childCtx = new LingoSDLRenderContext(context.Renderer, context.ImGuiViewPort, ImGui.GetWindowDrawList(), origin);
                            comp.Render(childCtx);
                        }
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }

            ImGui.EndChild();
            ImGui.PopID();
            return LingoSDLRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            ClearTabs();
            base.Dispose();
        }
    }

    public class SdlGfxTabItem : SdlGfxComponent, ILingoFrameworkGfxTabItem
    {
        public SdlGfxTabItem(SdlGfxFactory factory, LingoGfxTabItem tab) : base(factory)
        {
            tab.Init(this);
        }

        public string Title { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public ILingoGfxNode? Content { get; set; }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public override void Dispose() => base.Dispose();

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context) => LingoSDLRenderResult.RequireRender();
    }
}
