using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlScrollContainer : AbstSdlComponent, IAbstFrameworkScrollContainer, IDisposable
    {
        public AbstSdlScrollContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public float ScrollHorizontal { get; set; }
        public float ScrollVertical { get; set; }
        public bool ClipContents { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            _children.Remove(child);
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
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

            // child origin for nested components
            var scroll = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
            var childOrigin = ImGui.GetCursorScreenPos() - scroll;
            var childCtx = context.CreateNew(childOrigin);

            // render children & measure content extents
            float maxX = 0, maxY = 0;
            foreach (var child in _children)
            {
                if (child is AbstSdlComponent comp)
                {
                    comp.Render(childCtx);
                    maxX = MathF.Max(maxX, comp.X + comp.Width);
                    maxY = MathF.Max(maxY, comp.Y + comp.Height);
                }
            }

            // report content size to ImGui so it knows it should scroll
            ImGui.SetCursorPos(Vector2.Zero);
            ImGui.Dummy(new Vector2(maxX, maxY)); // <- drives scrollbar visibility

            // capture user scroll position for external access
            ScrollHorizontal = ImGui.GetScrollX();
            ScrollVertical = ImGui.GetScrollY();

            ImGui.EndChild();
            ImGui.PopID();

            return AbstSDLRenderResult.RequireRender();
        }



        public override void Dispose()
        {
            _children.Clear();
            base.Dispose();
        }
    }
}
