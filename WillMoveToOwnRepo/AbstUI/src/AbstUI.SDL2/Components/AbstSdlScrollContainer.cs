using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
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
            foreach (var child in _children)
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += -ScrollHorizontal;
                    ctx.OffsetY += -ScrollVertical;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }
        }

        public override void Dispose()
        {
            _children.Clear();
            base.Dispose();
        }
    }
}

