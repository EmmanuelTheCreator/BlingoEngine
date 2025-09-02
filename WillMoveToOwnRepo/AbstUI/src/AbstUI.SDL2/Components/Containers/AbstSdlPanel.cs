using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.FrameworkCommunication;
using AbstUI.SDL2.Bitmaps;

namespace AbstUI.SDL2.Components.Containers
{
    public class AbstSdlPanel : AbstSdlComponent, IAbstFrameworkPanel, IFrameworkFor<AbstPanel>, IDisposable, IHandleSdlEvent
    {

        private nint _texture;
        private int _texW;
        private int _texH;
        protected int _yOffset;
        protected int _xOffset;

        public AbstSdlPanel(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public AColor? BackgroundColor { get; set; }
        public AColor? BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public bool ClipChildren { get; set; }
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

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Remove(child))
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
                (child as IDisposable)?.Dispose();
            }
        }

        public void RemoveAll()
        {
            foreach (var child in _children)
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
                (child as IDisposable)?.Dispose();
            }
            _children.Clear();
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

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);

            if (BackgroundColor is { } bg)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderClear(context.Renderer);
            }
            else
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);
            }

            RenderChildren(context,0,0, w, h, _xOffset, _yOffset);

            if (BorderWidth > 0 && BorderColor is { } bc)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, bc.R, bc.G, bc.B, bc.A);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                for (int i = 0; i < (int)BorderWidth; i++)
                {
                    SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                    rect.x++; rect.y++; rect.w -= 2; rect.h -= 2;
                }
            }
            //if (_children.Count > 0)
            //{
            //    var t = new SdlTexture2D(_texture, w, h, "test");
            //    t.DebugWriteToDiskInc(context.Renderer);
            //}
            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            return _texture;
        }

        protected void RenderChildren(AbstSDLRenderContext context,int x, int y, int w, int h,int xOffset, int yOffset)
        {
            if (ClipChildren)
            {
                SDL.SDL_Rect clip = new SDL.SDL_Rect { x =x+ xOffset, y = y+yOffset, w = w, h = h };
                SDL.SDL_RenderSetClipRect(context.Renderer, ref clip);
            }

            foreach (var child in _children)
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += -X + xOffset;
                    ctx.OffsetY += -Y + yOffset;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }

            if (ClipChildren)
                SDL.SDL_RenderSetClipRect(context.Renderer, nint.Zero);
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
        public virtual bool CanHandleEvent(AbstSDLEvent e) => e.IsInside || !e.HasCoordinates;
        public virtual void HandleEvent(AbstSDLEvent e)
        {
            // Forward mouse events to children accounting for current scroll offset
            ContainerHelpers.HandleChildEvents(_children, e, -X - _xOffset - (int)Margin.Left, -Y - _yOffset - (int)Margin.Top);
            //var oriOffsetX = e.OffsetX;
            //var oriOffsetY = e.OffsetY;
            //for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            //{
            //    if (_children[i].FrameworkNode is not AbstSdlComponent comp ||
            //        comp is not IHandleSdlEvent handler ||
            //        !comp.Visibility)
            //        continue;
            //    if (comp is IAbstFrameworkLayoutNode layoutNode)
            //    {
            //        e.OffsetX = oriOffsetX + (int)Margin.Left - _xOffset - layoutNode.X;
            //        e.OffsetY = oriOffsetY + (int)Margin.Top - _yOffset - layoutNode.Y;
            //    }
            //    ContainerHelpers.HandleChildEvents(comp, e);
            //}
            //e.OffsetX = oriOffsetX;
            //e.OffsetY = oriOffsetY;
        }

    }
}
