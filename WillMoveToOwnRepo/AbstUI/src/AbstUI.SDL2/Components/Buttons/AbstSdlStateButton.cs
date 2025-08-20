using System.Numerics;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;
using AbstUI.Components.Buttons;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Buttons
{
    internal class AbstSdlStateButton : AbstSdlComponent, IAbstFrameworkStateButton, ISdlFocusable, IDisposable
    {
        private nint _textureOnPtr;
        private IAbstTexture2D? _textureOn;
        private nint _textureOffPtr;
        private IAbstTexture2D? _textureOff;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        private bool _renderedState;

        public AbstSdlStateButton(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        public IAbstTexture2D? TextureOn
        {
            get => _textureOn;
            set
            {
                _textureOn = value;
                if (_textureOnPtr != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_textureOnPtr);
                    _textureOnPtr = nint.Zero;
                }
                if (value is SdlImageTexture img)
                {
                    _textureOnPtr = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
                }
            }
        }

        public IAbstTexture2D? TextureOff
        {
            get => _textureOff;
            set
            {
                _textureOff = value;
                if (_textureOffPtr != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_textureOffPtr);
                    _textureOffPtr = nint.Zero;
                }
                if (value is SdlImageTexture img)
                {
                    _textureOffPtr = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
                }
            }
        }
        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;


        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            int w = (int)Width;
            int h = (int)Height;

            if (_texture == nint.Zero || w != _texW || h != _texH || _renderedState != _isOn)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
                _renderedState = _isOn;

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);

                if (_isOn && _textureOffPtr == nint.Zero)
                {
                    var accent = AbstDefaultColors.InputAccentColor;
                    SDL.SDL_SetRenderDrawColor(context.Renderer, accent.R, accent.G, accent.B, accent.A);
                    SDL.SDL_Rect bg = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                    SDL.SDL_RenderFillRect(context.Renderer, ref bg);
                }

                nint icon = _isOn ? _textureOnPtr != nint.Zero ? _textureOnPtr : _textureOffPtr
                                   : _textureOffPtr != nint.Zero ? _textureOffPtr : _textureOnPtr;

                if (icon != nint.Zero)
                {
                    SDL.SDL_QueryTexture(icon, out _, out _, out int iw, out int ih);
                    SDL.SDL_Rect dst = new SDL.SDL_Rect
                    {
                        x = (w - iw) / 2,
                        y = (h - ih) / 2,
                        w = iw,
                        h = ih
                    };
                    SDL.SDL_RenderCopy(context.Renderer, icon, nint.Zero, ref dst);
                }

                var borderColor = AbstDefaultColors.InputBorderColor;
                SDL.SDL_SetRenderDrawColor(context.Renderer, borderColor.R, borderColor.G, borderColor.B, borderColor.A);
                SDL.SDL_Rect border = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref border);

                SDL.SDL_SetRenderTarget(context.Renderer, prev);
            }

            return _texture;
        }

        public override void Dispose()
        {
            if (_textureOnPtr != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_textureOnPtr);
                _textureOnPtr = nint.Zero;
            }
            if (_textureOffPtr != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_textureOffPtr);
                _textureOffPtr = nint.Zero;
            }
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }

        public bool HasFocus => _focused;

        public void SetFocus(bool focus) => _focused = focus;
    }
}
