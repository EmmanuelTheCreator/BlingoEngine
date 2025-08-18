using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal abstract class AbstSdlScrollViewer : AbstSdlComponent, IHandleSdlEvent, IDisposable
    {
        protected AbstSdlScrollViewer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public AMargin Margin { get; set; } = AMargin.Zero;
        public float ScrollHorizontal { get; set; }
        public float ScrollVertical { get; set; }
        public bool ClipContents { get; set; } = true;

        private nint _texture;
        private int _texW;
        private int _texH;
        private float _lastScrollH;
        private float _lastScrollV;
        private bool _dragV;
        private bool _dragH;
        private int _dragStartX;
        private int _dragStartY;
        private float _scrollStartH;
        private float _scrollStartV;

        protected abstract void RenderContent(AbstSDLRenderContext context);

        protected virtual void HandleContentEvent(AbstSDLEvent e)
        {
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            int w = (int)Width;
            int h = (int)Height;
            const int sbSize = 16;

            bool needRender = _texture == nint.Zero || _texW != w || _texH != h ||
                               _lastScrollH != ScrollHorizontal || _lastScrollV != ScrollVertical;
            //if (needRender)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                SDL.SDL_SetRenderTarget(context.Renderer, _texture);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
                SDL.SDL_RenderClear(context.Renderer);

                if (ClipContents)
                {
                    SDL.SDL_Rect clip = new SDL.SDL_Rect { x = 0, y = 0, w = w - sbSize, h = h - sbSize };
                    SDL.SDL_RenderSetClipRect(context.Renderer, ref clip);
                }

                RenderContent(context);

                if (ClipContents)
                    SDL.SDL_RenderSetClipRect(context.Renderer, nint.Zero);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 200, 0, 0, 255);
                SDL.SDL_Rect vbar = new SDL.SDL_Rect { x = w - sbSize, y = 0, w = sbSize, h = h - sbSize };
                SDL.SDL_RenderFillRect(context.Renderer, ref vbar);
                SDL.SDL_Rect hbar = new SDL.SDL_Rect { x = 0, y = h - sbSize, w = w - sbSize, h = sbSize };
                SDL.SDL_RenderFillRect(context.Renderer, ref hbar);
                SDL.SDL_Rect corner = new SDL.SDL_Rect { x = w - sbSize, y = h - sbSize, w = sbSize, h = sbSize };
                SDL.SDL_RenderFillRect(context.Renderer, ref corner);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 50, 50, 50, 255);
                SDL.SDL_RenderDrawRect(context.Renderer, ref vbar);
                SDL.SDL_RenderDrawRect(context.Renderer, ref hbar);
                SDL.SDL_RenderDrawRect(context.Renderer, ref corner);

                SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);

                _texW = w;
                _texH = h;
                _lastScrollH = ScrollHorizontal;
                _lastScrollV = ScrollVertical;
            }
  
            return _texture;
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            HandleContentEvent(e);
            if (e.StopPropagation) return;

            ref var ev = ref e.Event;
            const int sbSize = 16;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                int lx = ev.button.x - (int)X;
                int ly = ev.button.y - (int)Y;
                if (lx >= Width - sbSize && ly < Height - sbSize)
                {
                    _dragV = true;
                    _dragStartY = ev.button.y;
                    _scrollStartV = ScrollVertical;
                    e.StopPropagation = true;
                }
                else if (ly >= Height - sbSize && lx < Width - sbSize)
                {
                    _dragH = true;
                    _dragStartX = ev.button.x;
                    _scrollStartH = ScrollHorizontal;
                    e.StopPropagation = true;
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                _dragV = _dragH = false;
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                if (_dragV)
                {
                    ScrollVertical = _scrollStartV + (ev.motion.y - _dragStartY);
                    ComponentContext.QueueRedraw(this);
                    e.StopPropagation = true;
                }
                else if (_dragH)
                {
                    ScrollHorizontal = _scrollStartH + (ev.motion.x - _dragStartX);
                    ComponentContext.QueueRedraw(this);
                    e.StopPropagation = true;
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {
                ScrollVertical -= ev.wheel.y * 20;
                ScrollHorizontal -= ev.wheel.x * 20;
                ComponentContext.QueueRedraw(this);
                e.StopPropagation = true;
            }
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }
    }
}

