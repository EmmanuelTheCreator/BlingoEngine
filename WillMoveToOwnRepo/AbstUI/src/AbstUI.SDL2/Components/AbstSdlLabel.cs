using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Texts;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlLabel : AbstSdlComponent, IAbstFrameworkLabel, IDisposable
    {
        public AbstSdlLabel(AbstSdlComponentFactory factory) : base(factory)
        {
            FontSize = 12;
        }
        public AMargin Margin { get; set; } = AMargin.Zero;

        public string Text { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public string? Font { get; set; }

        public AColor FontColor { get; set; } = AColors.Black;
        private int _lineHeight;
        public int LineHeight { get => _lineHeight; set => _lineHeight = value; }
        private ATextWrapMode _wrapMode;
        public ATextWrapMode WrapMode { get => _wrapMode; set => _wrapMode = value; }
        private AbstTextAlignment _textAlignment;
        public AbstTextAlignment TextAlignment { get => _textAlignment; set => _textAlignment = value; }

        public object FrameworkNode => this;

        public event Action? ValueChanged;

        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private string _renderedText = string.Empty;
        private int _measuredWidth;

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, Font, FontSize <= 0 ? 12 : FontSize);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility || string.IsNullOrEmpty(Text))
                return default;

            EnsureResources(context);

            if (_texture == nint.Zero || _renderedText != Text)
            {
                if (_texture != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_texture);
                    _texture = nint.Zero;
                }

                SDL.SDL_Color col = new SDL.SDL_Color { r = FontColor.R, g = FontColor.G, b = FontColor.B, a = FontColor.A };
                var surf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, Text, col);
                _texture = SDL.SDL_CreateTextureFromSurface(context.Renderer, surf);
                SDL.SDL_QueryTexture(_texture, out _, out _, out int w, out int h);
                SDL.SDL_FreeSurface(surf);

                _renderedText = Text;

                // Only override width if no explicit width was set
                if (Width <= 0)
                    Width = w;
                // Height is always dictated by the rendered texture
                Height = h;

                // Cache the measured width for alignment calculations
                _measuredWidth = w;
            }

            // Align texture within the available width
            float availableWidth = Width;
            float offset = 0;
            if (availableWidth > _measuredWidth)
            {
                offset = TextAlignment switch
                {
                    AbstTextAlignment.Center => (availableWidth - _measuredWidth) / 2f,
                    AbstTextAlignment.Right => availableWidth - _measuredWidth,
                    _ => 0
                };
            }
            ComponentContext.OffsetX = offset;

            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _font?.Release();
            base.Dispose();
        }
    }
}
