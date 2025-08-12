using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Core;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxScrollContainer : SdlGfxComponent, ILingoFrameworkGfxScrollContainer, IDisposable
    {
        public SdlGfxScrollContainer(SdlFactory factory) : base(factory)
        {
        }
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

        public override nint Render(LingoSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            ImGui.BeginChild("##scroll", new Vector2(Width, Height), ImGuiChildFlags.None);
            ImGui.SetScrollX(ScrollHorizontal);
            ImGui.SetScrollY(ScrollVertical);

            foreach (var child in _children)
            {
                if (child is SdlGfxComponent comp)
                    comp.Render(context);
            }

            ScrollHorizontal = ImGui.GetScrollX();
            ScrollVertical = ImGui.GetScrollY();
            ImGui.EndChild();
            ImGui.PopID();
            return nint.Zero;
        }

        public override void Dispose()
        {
            _children.Clear();
            base.Dispose();
        }
    }
}
