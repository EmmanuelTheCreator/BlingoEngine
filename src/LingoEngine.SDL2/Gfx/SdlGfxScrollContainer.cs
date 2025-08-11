using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxScrollContainer : ILingoFrameworkGfxScrollContainer, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;

        public SdlGfxScrollContainer(nint renderer)
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
        public float ScrollHorizontal { get; set; }
        public float ScrollVertical { get; set; }
        public bool ClipContents { get; set; }
        public object FrameworkNode => this;

        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public void RemoveItem(ILingoFrameworkGfxLayoutNode lingoFrameworkGfxNode)
        {
            _children.Remove(lingoFrameworkGfxNode);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _children.ToArray();

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            ImGui.BeginChild("##scroll", new Vector2(Width, Height), ImGuiChildFlags.None);
            ImGui.SetScrollX(ScrollHorizontal);
            ImGui.SetScrollY(ScrollVertical);

            foreach (var child in _children)
            {
                if (child.FrameworkNode is ISdlRenderElement renderable)
                    renderable.Render();
            }

            ScrollHorizontal = ImGui.GetScrollX();
            ScrollVertical = ImGui.GetScrollY();
            ImGui.EndChild();
            ImGui.PopID();
        }

        public void Dispose()
        {
            _children.Clear();
        }
    }
}
