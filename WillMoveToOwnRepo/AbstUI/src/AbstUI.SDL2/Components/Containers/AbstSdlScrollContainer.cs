using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Containers
{
    internal class AbstSdlScrollContainer : AbstSdlScrollViewer, IAbstFrameworkScrollContainer, IFrameworkFor<AbstScrollContainer>, IDisposable, IHandleSdlEvent
    {
        public AbstSdlScrollContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public object FrameworkNode => this;



        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(ComponentContext);
            }
        }

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Remove(child))
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            }
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        
        protected override void RenderContent(AbstSDLRenderContext context)
        {
            float maxX = 0, maxY = 0;
            foreach (var child in _children)
            {
                if (child.FrameworkNode is not AbstSdlComponent comp)
                    continue;
                
                var ctx = comp.ComponentContext;
                var oldOffX = ctx.OffsetX;
                var oldOffY = ctx.OffsetY;

                // Render child relative to this container's origin and scroll position
                ctx.OffsetX += - ScrollHorizontal;
                ctx.OffsetY +=  - ScrollVertical;
                ctx.RenderToTexture(context);

                ctx.OffsetX = oldOffX;
                ctx.OffsetY = oldOffY;

                maxX = MathF.Max(maxX, comp.X + comp.Width);
                maxY = MathF.Max(maxY, comp.Y + comp.Height);
            }

            ContentWidth = maxX;
            ContentHeight = maxY;
        }

        public bool CanHandleEvent(AbstSDLEvent e)
        {
            return true;
        }

        protected override void HandleContentEvent(AbstSDLEvent e)
        {
            // Forward mouse events to children accounting for current scroll offset
            Console.WriteLine(e.Event.type);
            ContainerHelpers.HandleChildEvents(_children, e, -ScrollHorizontal + X, -ScrollVertical + Y);

            //var oriX = e.OffsetX;
            //var oriY = e.OffsetY;
            //e.OffsetX = oriX + (int)(ScrollHorizontal - X);
            //e.OffsetY = oriY + (int)(ScrollVertical - Y);
            //for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            //{
            //    if (_children[i].FrameworkNode is not AbstSdlComponent comp ||
            //        comp is not IHandleSdlEvent handler ||
            //        !comp.Visibility)
            //        continue;

            //    ContainerHelpers.HandleChildEvents(comp, e);

            //    //ContainerHelpers.HandleChildEvents(comp, e,
            //    //   (int)ScrollHorizontal - (int)X,
            //    //   (int)ScrollVertical - (int)Y);
            //}
            //e.OffsetX = oriX;
            //e.OffsetY = oriY;
        }



        public override void Dispose()
        {
            foreach (var child in _children)
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            _children.Clear();
            base.Dispose();
        }
    }
}

