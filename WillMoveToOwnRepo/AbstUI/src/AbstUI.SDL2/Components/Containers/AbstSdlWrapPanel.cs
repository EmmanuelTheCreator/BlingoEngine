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
    public class AbstSdlWrapPanel : AbstSdlComponent, IAbstFrameworkWrapPanel, IFrameworkFor<AbstWrapPanel>, IDisposable, IHandleSdlEvent
    {
        private nint _texture;
        private int _texW;
        private int _texH;
        private AOrientation _orientation;
        public AOrientation Orientation
        {
            get => _orientation;
            set { if (_orientation != value) { _orientation = value; _layoutDirty = true; ComponentContext.QueueRedraw(this); } }
        }

        private APoint _itemMargin;
        public APoint ItemMargin
        {
            get => _itemMargin;
            set { if (!_itemMargin.Equals(value)) { _itemMargin = value; _layoutDirty = true; ComponentContext.QueueRedraw(this); } }
        }

        private AMargin _margin;
        public AMargin Margin
        {
            get => _margin;
            set { if (!_margin.Equals(value)) { _margin = value; _layoutDirty = true; ComponentContext.QueueRedraw(this); } }
        }

        public override float Width
        {
            get => base.Width;
            set { if (Math.Abs(base.Width - value) > 0.01f) { base.Width = value; _layoutDirty = true; ComponentContext.QueueRedraw(this); } }
        }

        public override float Height
        {
            get => base.Height;
            set { if (Math.Abs(base.Height - value) > 0.01f) { base.Height = value; _layoutDirty = true; ComponentContext.QueueRedraw(this); } }
        }

       
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

        private void RequestRedraw(IAbstSDLComponent component)
        {
            
            _layoutDirty = true;
        }

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
            if (!_layoutDirty  && !ComponentContext.RequireToRedraw) return;
            
            float availW = Math.Max(0, Width - Margin.Left - Margin.Right);
            float availH = Math.Max(0, Height - Margin.Top - Margin.Bottom);

            float startX = Margin.Left;
            float startY = Margin.Top;
            float curX = startX, curY = startY;
            float lineSize = 0;

            float contentW = 0f; // measured content (including margins)
            float contentH = 0f;



            foreach (var child in _children)
            {
                var node = child.Node;
                var m = node.Margin;
                float childW = node.Width + m.Left + m.Right;
                float childH = node.Height + m.Top + m.Bottom;

                float targetX, targetY;

                if (Orientation == AOrientation.Horizontal)
                {
                    if (availW > 0 && curX - startX + childW > availW)
                    {
                        curX = startX;
                        curY += lineSize + ItemMargin.Y;
                        lineSize = 0;
                    }
                    targetX = curX + m.Left;
                    targetY = curY + m.Top;
                    curX += childW + ItemMargin.X;
                    lineSize = Math.Max(lineSize, childH);
                }
                else
                {
                    if (availH > 0 && curY - startY + childH > availH)
                    {
                        curY = startY;
                        curX += lineSize + ItemMargin.X;
                        lineSize = 0;
                    }
                    targetX = curX + m.Left;
                    targetY = curY + m.Top;
                    curY += childH + ItemMargin.Y;
                    lineSize = Math.Max(lineSize, childW);
                }
                child.X = targetX;
                child.Y = targetY;
                //Console.WriteLine($"LAYOUT {child.Node.Name} -> {child.X},{child.Y}");

                if (node is IAbstFrameworkLayoutNode ln) { ln.X = targetX; ln.Y = targetY; }
                else if (node.FrameworkNode is AbstSdlComponent comp) { comp.X = targetX; comp.Y = targetY; }

                // Update measured content extents
                contentW = Math.Max(contentW, (targetX - Margin.Left) + (node.Width + m.Left + m.Right));
                contentH = Math.Max(contentH, (targetY - Margin.Top) + (node.Height + m.Top + m.Bottom));

                child.IsDirty = false;
            }

            // Report desired size to parent (scroll containers use this)
            ComponentContext.TargetWidth = (int)Math.Ceiling(contentW + Margin.Left + Margin.Right);
            ComponentContext.TargetHeight = (int)Math.Ceiling(contentH + Margin.Top + Margin.Bottom);

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
                if (child.Node.FrameworkNode is not AbstSdlComponent comp) continue;

                var ctx = comp.ComponentContext;

                // place the child *inside this panel's texture*
                var oldX = ctx.X; var oldY = ctx.Y;
                ctx.X = (int)(child.X + Margin.Left);
                ctx.Y = (int)(child.Y + Margin.Top);

                // (no Offset push here)
                //Console.WriteLine($"WRAP_PANEL BLIT {comp.Name} at {comp.X},{comp.Y} into {Name} at {X},{Y}  off=({ctx.OffsetX},{ctx.OffsetY}) {child.Node.Name}");
                comp.ComponentContext.RenderToTexture(context);

                ctx.X = oldX; ctx.Y = oldY;
            }




            // also make sure our target size is set so parent blits us correctly
            ComponentContext.TargetWidth = (int)Width;
            ComponentContext.TargetHeight = (int)Height;



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
