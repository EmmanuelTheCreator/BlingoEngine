using System;
using System.Collections.Generic;
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

            // TODO: draw panel directly using ImGui
            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            RemoveAll();
            base.Dispose();
        }
    }
}
