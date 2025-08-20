using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using System;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Components.Containers
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
        public AbstScrollbarMode ScollbarModeH { get; set; }  = AbstScrollbarMode.Auto;
        public AbstScrollbarMode ScollbarModeV { get; set; } = AbstScrollbarMode.Auto;
        protected float ContentWidth { get; set; }
        protected float ContentHeight { get; set; }

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
        private float _maxScrollH;
        private float _maxScrollV;
        private float _trackW;
        private float _trackH;
        private float _handleW;
        private float _handleH;
        private float _dragRatioH;
        private float _dragRatioV;

        public AColor Color_Handle { get; set; } = AColor.FromRGBA(100, 100, 100);
        public AColor Color_Bars_Bg { get; set; } = AColor.FromRGBA(255, 255, 255, 0);
        public AColor Color_ScollBorder { get; set; } = AColor.FromRGBA(255, 255, 255, 0);
        public AColor BackgroundColor { get; set; } = AColor.FromRGBA(255, 255, 255, 0);
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
            const int arrowSize = 8;

            int viewW = w - sbSize;
            int viewH = h - sbSize;

            _maxScrollH = MathF.Max(0, ContentWidth - viewW);
            _maxScrollV = MathF.Max(0, ContentHeight - viewH);
            var showHScrollBar = ScollbarModeH == AbstScrollbarMode.AlwaysVisible || (ScollbarModeH == AbstScrollbarMode.Auto && _maxScrollH > 0);
            var showVScrollBar = ScollbarModeV == AbstScrollbarMode.AlwaysVisible || (ScollbarModeV == AbstScrollbarMode.Auto && _maxScrollV > 0);
            

            if (ScrollHorizontal < 0) ScrollHorizontal = 0; else if (ScrollHorizontal > _maxScrollH) ScrollHorizontal = _maxScrollH;
            if (ScrollVertical < 0) ScrollVertical = 0; else if (ScrollVertical > _maxScrollV) ScrollVertical = _maxScrollV;

            _trackW = viewW - arrowSize * 2;
            _trackH = viewH - arrowSize * 2;
            _handleW = _maxScrollH > 0 ? MathF.Max(20, _trackW * viewW / ContentWidth) : _trackW;
            _handleH = _maxScrollV > 0 ? MathF.Max(20, _trackH * viewH / ContentHeight) : _trackH;
            _handleW = MathF.Min(_handleW, _trackW);
            _handleH = MathF.Min(_handleH, _trackH);
            _dragRatioH = _maxScrollH / MathF.Max(1, _trackW - _handleW);
            _dragRatioV = _maxScrollV / MathF.Max(1, _trackH - _handleH);

            bool needRender = _texture == nint.Zero || _texW != w || _texH != h ||
                               _lastScrollH != ScrollHorizontal || _lastScrollV != ScrollVertical;
            //if (needRender)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);

                var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                //SDL.SDL_SetTextureBlendMode(_texture, SDL_BlendMode.SDL_BLENDMODE_NONE);
                SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
                SDL.SDL_RenderClear(context.Renderer);

                if (ClipContents)
                {
                    SDL.SDL_Rect clip = new SDL.SDL_Rect { x = 0, y = 0, w = viewW, h = viewH };
                    SDL.SDL_RenderSetClipRect(context.Renderer, ref clip);
                }

                RenderContent(context);

                if (ClipContents)
                    SDL.SDL_RenderSetClipRect(context.Renderer, nint.Zero);

                // Draw bg scrollbars
                SDL.SDL_SetRenderDrawColor(context.Renderer, Color_Bars_Bg.R, Color_Bars_Bg.G, Color_Bars_Bg.B, Color_Bars_Bg.A);
               
                SDL.SDL_Rect hbar = new SDL.SDL_Rect { x = 0, y = h - sbSize, w = viewW, h = sbSize };
                if (showHScrollBar)
                    SDL.SDL_RenderFillRect(context.Renderer, ref hbar);
                SDL.SDL_Rect vbar = new SDL.SDL_Rect { x = w - sbSize, y = 0, w = sbSize, h = viewH };
                if (showVScrollBar)
                    SDL.SDL_RenderFillRect(context.Renderer, ref vbar);
                
                SDL.SDL_Rect corner = new SDL.SDL_Rect { x = w - sbSize, y = h - sbSize, w = sbSize, h = sbSize };
                SDL.SDL_RenderFillRect(context.Renderer, ref corner);

                // Draw handle
                float vPos = _maxScrollV > 0 ? ScrollVertical / _maxScrollV * (_trackH - _handleH) : 0;
                float hPos = _maxScrollH > 0 ? ScrollHorizontal / _maxScrollH * (_trackW - _handleW) : 0;
                SDL.SDL_SetRenderDrawColor(context.Renderer, Color_Handle.R, Color_Handle.G, Color_Handle.B, Color_Handle.A);
                if (showHScrollBar)
                {
                    SDL.SDL_Rect hhandle = new SDL.SDL_Rect { x = (int)(arrowSize + hPos) + 2, y = h - sbSize + 2, w = (int)_handleW - 4, h = sbSize - 4 };
                    SDL.SDL_RenderFillRect(context.Renderer, ref hhandle);
                }
                if (showVScrollBar)
                {
                    SDL.SDL_Rect vhandle = new SDL.SDL_Rect { x = w - sbSize + 2, y = (int)(arrowSize + vPos) + 2, w = sbSize - 4, h = (int)_handleH - 4 };
                    SDL.SDL_RenderFillRect(context.Renderer, ref vhandle);
                }
                
                // draw arrows
                SDL.SDL_SetRenderDrawColor(context.Renderer, Color_Handle.R, Color_Handle.G, Color_Handle.B, Color_Handle.A);
                int cx = w - sbSize / 2;
                int cy = h - sbSize / 2;
                int ah = arrowSize - 4;
                if (showHScrollBar)
                {
                    for (int i = 0; i < ah; i++)
                        SDL.SDL_RenderDrawLine(context.Renderer, 3 + i, cy - i, 3 + i, cy + i);
                    for (int i = 0; i < ah; i++)
                        SDL.SDL_RenderDrawLine(context.Renderer, viewW - 3 - i, cy - i, viewW - 3 - i, cy + i);
                }
                if (showVScrollBar)
                {
                    for (int i = 0; i < ah; i++)
                        SDL.SDL_RenderDrawLine(context.Renderer, cx - i, 3 + i, cx + i, 3 + i);
                    for (int i = 0; i < ah; i++)
                        SDL.SDL_RenderDrawLine(context.Renderer, cx - i, viewH - 3 - i, cx + i, viewH - 3 - i);
                }
               
                SDL.SDL_SetRenderDrawColor(context.Renderer, Color_ScollBorder.R, Color_ScollBorder.G, Color_ScollBorder.B, Color_ScollBorder.A);
                if (showHScrollBar)
                    SDL.SDL_RenderDrawRect(context.Renderer, ref hbar);
                if (showVScrollBar)
                    SDL.SDL_RenderDrawRect(context.Renderer, ref vbar);
               
                SDL.SDL_RenderDrawRect(context.Renderer, ref corner);

                SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);

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
            const int arrowSize = 8;
            const int step = 20;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                int lx = ev.button.x - (int)X;
                int ly = ev.button.y - (int)Y;
                int viewW = (int)Width - sbSize;
                int viewH = (int)Height - sbSize;
                if (lx >= Width - sbSize && ly < Height - sbSize)
                {
                    if (ly < arrowSize)
                    {
                        ScrollVertical = Math.Clamp(ScrollVertical - step, 0, _maxScrollV);
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                    else if (ly >= viewH - arrowSize)
                    {
                        ScrollVertical = Math.Clamp(ScrollVertical + step, 0, _maxScrollV);
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                    else
                    {
                        _dragV = true;
                        _dragStartY = ev.button.y;
                        _scrollStartV = ScrollVertical;
                        e.StopPropagation = true;
                    }
                }
                else if (ly >= Height - sbSize && lx < Width - sbSize)
                {
                    if (lx < arrowSize)
                    {
                        ScrollHorizontal = Math.Clamp(ScrollHorizontal - step, 0, _maxScrollH);
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                    else if (lx >= viewW - arrowSize)
                    {
                        ScrollHorizontal = Math.Clamp(ScrollHorizontal + step, 0, _maxScrollH);
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                    else
                    {
                        _dragH = true;
                        _dragStartX = ev.button.x;
                        _scrollStartH = ScrollHorizontal;
                        e.StopPropagation = true;
                    }
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
                    ScrollVertical = _scrollStartV + (ev.motion.y - _dragStartY) * _dragRatioV;
                    if (ScrollVertical < 0) ScrollVertical = 0; else if (ScrollVertical > _maxScrollV) ScrollVertical = _maxScrollV;
                    ComponentContext.QueueRedraw(this);
                    e.StopPropagation = true;
                }
                else if (_dragH)
                {
                    ScrollHorizontal = _scrollStartH + (ev.motion.x - _dragStartX) * _dragRatioH;
                    if (ScrollHorizontal < 0) ScrollHorizontal = 0; else if (ScrollHorizontal > _maxScrollH) ScrollHorizontal = _maxScrollH;
                    ComponentContext.QueueRedraw(this);
                    e.StopPropagation = true;
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {
                ScrollVertical = Math.Clamp(ScrollVertical - ev.wheel.y * 20, 0, _maxScrollV);
                ScrollHorizontal = Math.Clamp(ScrollHorizontal - ev.wheel.x * 20, 0, _maxScrollH);
                ComponentContext.QueueRedraw(this);
                //e.StopPropagation = true;
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

