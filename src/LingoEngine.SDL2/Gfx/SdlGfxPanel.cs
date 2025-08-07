using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxPanel : ILingoFrameworkGfxPanel, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxPanel(nint renderer)
        {
            _renderer = renderer;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public LingoColor? BackgroundColor { get; set; }
        public LingoColor? BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public object FrameworkNode => this;

        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _children.ToArray();

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            _children.Remove(child);
        }

        public void RemoveAll()
        {
            _children.Clear();
        }

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);

            var pushed = 0;
            if (BackgroundColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.ChildBg, BackgroundColor.Value.ToImGuiColor());
                pushed++;
            }
            if (BorderColor.HasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, BorderColor.Value.ToImGuiColor());
                pushed++;
            }
            if (BorderWidth > 0)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, BorderWidth);
            }

            ImGui.BeginChild("##panel", new Vector2(Width, Height), BorderWidth > 0);
            foreach (var child in _children)
            {
                if (child.FrameworkNode is ISdlRenderElement renderable)
                    renderable.Render();
            }
            ImGui.EndChild();

            if (BorderWidth > 0)
                ImGui.PopStyleVar();
            if (pushed > 0)
                ImGui.PopStyleColor(pushed);

            ImGui.PopID();
        }

        public void Dispose()
        {
            RemoveAll();
        }
    }
}
