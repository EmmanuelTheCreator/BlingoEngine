using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Texts;
using System.Runtime.InteropServices;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlLabel : AbstSdlComponent, IAbstFrameworkLabel, IDisposable
    {
        private float _lastWidth;
        private float _lastHeight;
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

            if (_texture == nint.Zero || _renderedText != Text || _lastWidth != Width || _lastHeight != Height)
            {
                if (_texture != nint.Zero) { SDL.SDL_DestroyTexture(_texture); _texture = nint.Zero; }

                var (tw, th) = MeasureText(Text);

                // decide final box
                float boxW = Width > 0 ? Width : tw;
                float boxH = Height > 0 ? Height : th;

                // build texture
                var (tex, textW, textH) = CreateTextTextureBox(context, Text, (int)boxW, (int)boxH, TextAlignment);
                _texture = tex;
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                // update sizes
                if (Width <= 0) Width = boxW;
                if (Height <= 0) Height = boxH;

                _measuredWidth = textW;
                _renderedText = Text;
                _lastWidth = Width; _lastHeight = Height;
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
        private (nint tex, int textW, int textH) CreateTextTextureBox(
     AbstSDLRenderContext context, string text, int boxW, int boxH, AbstTextAlignment align)
        {
            // render glyphs
            SDL.SDL_Color col = FontColor.ToSDLColor();
            nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, text, col);
            if (textSurf == nint.Zero) throw new Exception(SDL_ttf.TTF_GetError());
            var ts = Marshal.PtrToStructure<SDL.SDL_Surface>(textSurf);
            int tw = ts.w, th = ts.h;

            // fallback sizes
            if (boxW <= 0) boxW = tw;
            if (boxH <= 0) boxH = th;

            // box surface
            var FMT = SDL.SDL_PIXELFORMAT_RGBA8888;
            nint box = SDL.SDL_CreateRGBSurfaceWithFormat(0, boxW, boxH, 32, FMT);
            if (box == nint.Zero) { SDL.SDL_FreeSurface(textSurf); throw new Exception(SDL.SDL_GetError()); }
            SDL.SDL_FillRect(box, IntPtr.Zero, 0x00000000);

            // dst placement
            int dstX = align switch
            {
                AbstTextAlignment.Center => Math.Max(0, (boxW - tw) / 2),
                AbstTextAlignment.Right => Math.Max(0, boxW - tw),
                _ => 0
            };
            int dstY = Math.Max(0, (boxH - th) / 2);

            var dst = new SDL.SDL_Rect { x = dstX, y = dstY, w = tw, h = th };
            SDL.SDL_BlitSurface(textSurf, IntPtr.Zero, box, ref dst);

            // texture
            nint tex = SDL.SDL_CreateTextureFromSurface(context.Renderer, box);
            SDL.SDL_SetTextureBlendMode(tex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL.SDL_FreeSurface(textSurf);
            SDL.SDL_FreeSurface(box);
            return (tex, tw, th);
        }

        private (int w, int h) MeasureText(string text)
        {
            SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, text, out int w, out int h);
            return (w, h);
        }

    }
}
