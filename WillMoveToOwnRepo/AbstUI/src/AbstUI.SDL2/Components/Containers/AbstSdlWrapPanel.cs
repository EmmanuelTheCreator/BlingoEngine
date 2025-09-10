using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components.Containers
{
    internal class AbstSdlWrapPanel : AbstSdlComponent, IAbstFrameworkWrapPanel, IFrameworkFor<AbstWrapPanel>, IDisposable, IHandleSdlEvent
    {
        private nint _texture;
        private int _texW;
        private int _texH;
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private class ChildData
        {
            public ChildData(IAbstFrameworkNode node)
            {
                Node = node;
                IsDirty = true;
            }

            public IAbstFrameworkNode Node { get; }
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsDirty { get; set; }
        }

        private readonly List<ChildData> _children = new();
        private bool _layoutDirty = true;

        public AbstSdlWrapPanel(AbstSdlComponentFactory factory, AOrientation orientation) : base(factory)
        {
            Orientation = orientation;
            ItemMargin = new APoint(0, 0);
            Margin = AMargin.Zero;
            ComponentContext.OnRequestRedraw += RequestRedraw; 
        }
        public override void Dispose()
        {
            ComponentContext.OnRequestRedraw -= RequestRedraw;
            RemoveAll();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }

        private void RequestRedraw(IAbstSDLComponent component) => _layoutDirty = true;

        public void AddItem(IAbstFrameworkNode child)
        {
            if (_children.Any(c => ReferenceEquals(c.Node, child)))
                return;

            _children.Add(new ChildData(child));
            if (child.FrameworkNode is AbstSdlComponent comp)
                comp.ComponentContext.SetParents(ComponentContext);

            _layoutDirty = true;
            RequestRedraw(this);
        }

        public void RemoveItem(IAbstFrameworkNode child)
        {
            var entry = _children.FirstOrDefault(c => ReferenceEquals(c.Node, child));
            if (entry == null)
                return;

            if (entry.Node.FrameworkNode is AbstSdlComponent comp)
                comp.ComponentContext.SetParents(null);

            _children.Remove(entry);
            _layoutDirty = true;
            RequestRedraw(this);
        }

        public IEnumerable<IAbstFrameworkNode> GetItems() => _children.Select(c => c.Node).ToArray();

        public IAbstFrameworkNode? GetItem(int index)
        {
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index].Node;
        }

        public void RemoveAll()
        {
            foreach (var child in _children)
                if (child.Node.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            _children.Clear();
            _layoutDirty = true;
            RequestRedraw(this);
        }

        public void MarkChildDirty(IAbstFrameworkNode child)
        {
            var entry = _children.FirstOrDefault(c => ReferenceEquals(c.Node, child));
            if (entry != null)
            {
                entry.IsDirty = true;
                _layoutDirty = true;
            }
        }

        private void RecalculateLayout()
        {
            if (!_layoutDirty)
                return;

            float curX = 0;
            float curY = 0;
            float lineSize = 0;
            foreach (var child in _children)
            {
                var node = child.Node;
                var margin = node.Margin;
                float childW = node.Width + margin.Left + margin.Right;
                float childH = node.Height + margin.Top + margin.Bottom;

                float targetX;
                float targetY;

                if (Orientation == AOrientation.Horizontal)
                {
                    if (curX + childW > Width)
                    {
                        curX = 0;
                        curY += lineSize + ItemMargin.Y;
                        lineSize = 0;
                    }
                    targetX = curX + margin.Left;
                    targetY = curY + margin.Top;
                    curX += childW + ItemMargin.X;
                    lineSize = Math.Max(lineSize, childH);
                }
                else
                {
                    if (curY + childH > Height)
                    {
                        curY = 0;
                        curX += lineSize + ItemMargin.X;
                        lineSize = 0;
                    }
                    targetX = curX + margin.Left;
                    targetY = curY + margin.Top;
                    curY += childH + ItemMargin.Y;
                    lineSize = Math.Max(lineSize, childW);
                }

                child.X = targetX;
                child.Y = targetY;

                if (node is IAbstFrameworkLayoutNode layout)
                {
                    layout.X = targetX;
                    layout.Y = targetY;
                }
                else if (node.FrameworkNode is AbstSdlComponent comp)
                {
                    comp.X = targetX;
                    comp.Y = targetY;
                }

                child.IsDirty = false;
            }

            _layoutDirty = false;
        }

      
      

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            int w = (int)Width;
            int h = (int)Height;
            if (_texture == nint.Zero || w != _texW || h != _texH)
            {
                if (_texture != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_texture);
                }
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                _texW = w;
                _texH = h;
            }

            RecalculateLayout();

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(context.Renderer);

            foreach (var child in _children)
            {
                if (child.Node.FrameworkNode is not AbstSdlComponent comp)
                    continue;
                comp.ComponentContext.RenderToTexture(context);
            }

            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            return _texture;
        }
        public bool CanHandleEvent(AbstSDLEvent e) => e.IsInside || !e.HasCoordinates;
        public void HandleEvent(AbstSDLEvent e)
        {
            RecalculateLayout();
            var nodes = _children.Select(c => c.Node).ToList();
            ContainerHelpers.HandleChildEvents(nodes, e, -X - (int)Margin.Left, -Y - (int)Margin.Top);
            //            // Forward mouse events to children accounting for current scroll offset
            //            var oriX = e.OffsetX;
            //            var oriY = e.OffsetY;
            //            for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            //            {
            //                if (_children[i].FrameworkNode is not AbstSdlComponent comp ||
            //                    comp is not IHandleSdlEvent handler ||
            //                    !comp.Visibility)
            //                    continue;

            //                e.OffsetX = oriX + (int)Margin.Left;
            //                e.OffsetY = oriY + (int)Margin.Top;
            //#if DEBUG
            //                if (e.Event.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
            //                {

            //                }
            //#endif
            //                ContainerHelpers.HandleChildEvents(comp, e);

            //            }
            //            e.OffsetX = oriX;
            //            e.OffsetY = oriY;
        }


    }
}
