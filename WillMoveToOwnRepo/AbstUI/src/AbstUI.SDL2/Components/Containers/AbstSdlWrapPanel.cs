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
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkNode> _children = new();

        public AbstSdlWrapPanel(AbstSdlComponentFactory factory, AOrientation orientation) : base(factory)
        {
            Orientation = orientation;
            ItemMargin = new APoint(0, 0);
            Margin = AMargin.Zero;
        }

        public void AddItem(IAbstFrameworkNode child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(ComponentContext);
            }
        }

        public void RemoveItem(IAbstFrameworkNode child)
        {
            if (_children.Remove(child))
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            }
        }

        public IEnumerable<IAbstFrameworkNode> GetItems() => _children.ToArray();

        public IAbstFrameworkNode? GetItem(int index)
        {
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index];
        }

        public void RemoveAll()
        {
            foreach (var child in _children)
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            _children.Clear();
        }

        public override void Dispose()
        {
            RemoveAll();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }

        private nint _texture;
        private int _texW;
        private int _texH;

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

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(context.Renderer);

            float curX = 0;
            float curY = 0;
            float lineSize = 0;
            foreach (var child in _children)
            {
                var margin = child.Margin;
                float childW = child.Width + margin.Left + margin.Right;
                float childH = child.Height + margin.Top + margin.Bottom;

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

                var comp = child.FrameworkNode as AbstSdlComponent;

                if (child is IAbstFrameworkLayoutNode layout)
                {
                    layout.X = targetX;
                    layout.Y = targetY;
                }
                else if (comp != null)
                {
                    comp.X = targetX;
                    comp.Y = targetY;
                }

                comp?.ComponentContext.RenderToTexture(context);
            }

            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            return _texture;
        }
        public bool CanHandleEvent(AbstSDLEvent e) => e.IsInside || !e.HasCoordinates;
        public void HandleEvent(AbstSDLEvent e)
        {
            //ContainerHelpers.HandleChildEvents(_children,e, Margin.Left, Margin.Top);
            ContainerHelpers.HandleChildEvents(_children, e, -X  - (int)Margin.Left, -Y - (int)Margin.Top);
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
