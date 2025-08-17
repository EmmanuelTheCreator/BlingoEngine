using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiPanel : AbstImGuiComponent, IAbstFrameworkPanel, IDisposable
    {
        public AbstImGuiPanel(AbstImGuiComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public AColor? BackgroundColor { get; set; }
        public AColor? BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Remove(child))
                (child as IDisposable)?.Dispose();
        }

        public void RemoveAll()
        {
            foreach (var child in _children)
                (child as IDisposable)?.Dispose();
            _children.Clear();
        }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            var drawList = context.ImDrawList;
            var p0 = context.Origin + new Vector2(X, Y);
            var p1 = p0 + new Vector2(Width, Height);

            if (BackgroundColor is { } bg)
                drawList.AddRectFilled(p0, p1, global::ImGuiNET.ImGui.GetColorU32(bg.ToImGuiColor()));

            foreach (var child in _children)
            {
                if (child.FrameworkNode is AbstImGuiComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += -X;
                    ctx.OffsetY += -Y;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }

            if (BorderWidth > 0 && BorderColor is { } bc)
                drawList.AddRect(p0, p1, global::ImGuiNET.ImGui.GetColorU32(bc.ToImGuiColor()), 0f, global::ImGuiNET.ImDrawFlags.None, BorderWidth);

            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            RemoveAll();
            base.Dispose();
        }
    }
}
