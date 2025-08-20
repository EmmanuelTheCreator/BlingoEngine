using System;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlColorPicker : AbstSdlComponent, IAbstFrameworkColorPicker, ISdlFocusable, IDisposable
    {
        public AbstSdlColorPicker(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color;
        private AColor _renderedColor;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        public AColor Color
        {
            get => _color;
            set
            {
                if (!_color.Equals(value))
                {
                    _color = value;
                    ValueChanged?.Invoke();
                }
            }
        }

        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility)
                return default;

            int w = (int)Width;
            int h = (int)Height;

            if (_texture == nint.Zero || _texW != w || _texH != h || !_renderedColor.Equals(Color))
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer,
                    SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
                    w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                SDL.SDL_SetRenderDrawColor(context.Renderer, Color.R, Color.G, Color.B, Color.A);
                SDL.SDL_RenderClear(context.Renderer);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                SDL.SDL_Rect r = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref r);

                SDL.SDL_SetRenderTarget(context.Renderer, prev);
                _texW = w;
                _texH = h;
                _renderedColor = Color;
            }

            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            base.Dispose();
        }
    }
}
