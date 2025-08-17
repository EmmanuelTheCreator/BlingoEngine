using System.Numerics;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlStateButton : AbstSdlComponent, IAbstFrameworkStateButton, IDisposable
    {
        private nint _textureOnPtr;
        private IAbstTexture2D? _textureOn;
        private nint _textureOffPtr;
        private IAbstTexture2D? _textureOff;

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
            return default;
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
            base.Dispose();
        }
    }
}
