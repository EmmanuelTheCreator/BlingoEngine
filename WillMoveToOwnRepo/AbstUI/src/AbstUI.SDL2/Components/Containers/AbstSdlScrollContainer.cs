using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Containers
{
    internal class AbstSdlScrollContainer : AbstSdlScrollViewer, IAbstFrameworkScrollContainer, IDisposable
    {
        public AbstSdlScrollContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

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

        protected override void RenderContent(AbstSDLRenderContext context)
        {
            float maxX = 0, maxY = 0;
            foreach (var child in _children)
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;

                    // Render child relative to this container's origin and scroll position
                    ctx.OffsetX += -X - ScrollHorizontal;
                    ctx.OffsetY += -Y - ScrollVertical;
                    ctx.RenderToTexture(context);

                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;

                    maxX = MathF.Max(maxX, comp.X + comp.Width);
                    maxY = MathF.Max(maxY, comp.Y + comp.Height);
                }
            }

            ContentWidth = maxX;
            ContentHeight = maxY;
        }

        public override void Dispose()
        {
            _children.Clear();
            base.Dispose();
        }
    }
}

