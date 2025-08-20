using System;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlInputCheckbox : AbstSdlComponent, IAbstFrameworkInputCheckbox, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        public AbstSdlInputCheckbox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private bool _checked;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        private bool _prevChecked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT &&
                HitTest(ev.button.x, ev.button.y))
            {
                Factory.FocusManager.SetFocus(this);
                Checked = !Checked;
                e.StopPropagation = true;
            }
        }

        private bool HitTest(int x, int y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            int w = (int)Width;
            int h = (int)Height;

            // create texture if needed
            if (_texture == nint.Zero || w != _texW || h != _texH || _prevChecked != _checked)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
                _prevChecked = _checked;

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                // clear with transparent background
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);

                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };

                if (_checked)
                {
                    var accent = AbstDefaultColors.InputAccentColor;
                    SDL.SDL_SetRenderDrawColor(context.Renderer, accent.R, accent.G, accent.B, accent.A);
                    SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                }

                // draw border
                var border = AbstDefaultColors.InputBorderColor;
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                SDL.SDL_SetRenderTarget(context.Renderer, prev);
            }

            return _texture;
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
