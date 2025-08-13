using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxScrollContainer : SdlGfxComponent, ILingoFrameworkGfxScrollContainer, IDisposable
    {
        public SdlGfxScrollContainer(SdlGfxFactory factory) : base(factory)
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

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            var screenPos = context.Origin + new Vector2(X, Y);
            ImGui.SetCursorScreenPos(screenPos);

            ImGui.PushID(Name);

            var childFlags = ImGuiChildFlags.None;
            var winFlags = ImGuiWindowFlags.None; // add Always*Scrollbar if you want to force

            // clip border if you like
            if (ClipContents) childFlags |= ImGuiChildFlags.Borders;

            ImGui.BeginChild("##scroll", new Vector2(Width, Height), childFlags, winFlags);

            // apply incoming scroll
            ImGui.SetScrollX(ScrollHorizontal);
            ImGui.SetScrollY(ScrollVertical);

            // child origin for nested components
            var childOrigin = ImGui.GetCursorScreenPos();
            var childCtx = new LingoSDLRenderContext(
                context.Renderer,
                context.ImGuiViewPort,
                ImGui.GetWindowDrawList(),
                childOrigin);

            // render children & measure content extents
            float maxX = 0, maxY = 0;
            foreach (var child in _children)
            {
                if (child is SdlGfxComponent comp)
                {
                    comp.Render(childCtx);
                    maxX = MathF.Max(maxX, comp.X + comp.Width);
                    maxY = MathF.Max(maxY, comp.Y + comp.Height);
                }
            }

            // report content size to ImGui so it knows it should scroll
            ImGui.SetCursorScreenPos(childOrigin);
            ImGui.Dummy(new Vector2(maxX, maxY)); // <- drives scrollbar visibility

            // read back scroll (after items)
            ScrollHorizontal = ImGui.GetScrollX();
            ScrollVertical = ImGui.GetScrollY();

            ImGui.EndChild();
            ImGui.PopID();

            return LingoSDLRenderResult.RequireRender();
        }



        public override void Dispose()
        {
            _children.Clear();
            base.Dispose();
        }
    }
}
