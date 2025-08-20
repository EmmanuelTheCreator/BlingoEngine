using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Texts;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace AbstUI.SDL2.Components.Texts
{
    public class AbstSdlLabel : AbstSdlComponent, IAbstFrameworkLabel, IDisposable
    {
        private float _lastWidth;
        private float _lastHeight;
        public AbstSdlLabel(AbstSdlComponentFactory factory) : base(factory)
        {
            FontSize = 12;
        }
        public AMargin Margin { get; set; } = AMargin.Zero;

        public string Text { get => _text; set => _text = CleanText(value); } // we need to trim the last newline to avoid text centered not rendering correctly
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
        private int _textWidth;
        private int _textHeight;
        private string _text = string.Empty;

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, Font, FontSize <= 0 ? 12 : FontSize);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility || string.IsNullOrEmpty(Text) )
                return default;

            EnsureResources(context);
            if (_font == null) return default;

            if (_texture == nint.Zero || _renderedText != Text || _lastWidth != Width || _lastHeight != Height)
            {
                if (_texture != nint.Zero) { SDL.SDL_DestroyTexture(_texture); _texture = nint.Zero; }

                if (_renderedText != Text)
                    MeasureSDLText(_font!.FontHandle, Text);

                // decide final box
                float boxW = Width > 0 ? Width : _textWidth;
                float boxH = Height > 0 ? Height : _textHeight;

                // build texture
                // render glyphs
                SDL.SDL_Color col = FontColor.ToSDLColor();
                var (tex, textW, textH) = CreateTextTextureBox(context.Renderer, _font!.FontHandle,FontSize, Text, (int)boxW, (int)boxH, TextAlignment, col);
                _texture = tex;
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL.SDL_FreeSurface(tex);
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
        public static (nint tex,int textW, int textH) CreateTextTextureBox(nint renderer, nint fontHandle, int fontSize, string text, int boxW, int boxH, AbstTextAlignment align, SDL.SDL_Color col, bool wordWrap = true)
        {

            var hasLineBreak = text.IndexOf('\n') >= 0;
            nint textSurf =  wordWrap || hasLineBreak
                ? SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(fontHandle, text, col, (uint)(boxW+20)) // for some reason we need to add an offset of 20px to the width, otherwise it does not wrap correctly
                : SDL_ttf.TTF_RenderUTF8_Blended(fontHandle, text, col);
            //nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(fontHandle, text, col);
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
            SDL.SDL_FillRect(box, nint.Zero, 0x00000000);
            //SDL.SDL_FillRect(box, nint.Zero, 0xFFFFFFFF);

            // dst placement
            int dstX = align switch
            {
                AbstTextAlignment.Center => Math.Max(0, (boxW - tw) / 2),
                AbstTextAlignment.Right => Math.Max(0, boxW - tw),
                _ => 0
            };

            int dstY = Math.Max(0, (boxH - th) / 2);
            int ascent = SDL_ttf.TTF_FontAscent(fontHandle);
            int descent = SDL_ttf.TTF_FontDescent(fontHandle);
            var kerning = SDL_ttf.TTF_GetFontKerning(fontHandle);
            var lineGap = SDL_ttf.TTF_FontLineSkip(fontHandle);
            var fontHeight = SDL_ttf.TTF_FontHeight(fontHandle);
            int tightHeight = fontHeight - ascent ;
            var sdlFixY = tightHeight-1; 
            //var sdlFixY = (int)MathF.Floor(fontSize/7); // 6px, SDL_ttf has a bug that it renders text too high, so we need to shift it up a bit
            var dst = new SDL.SDL_Rect { x = dstX, y = dstY- sdlFixY, w = tw, h = th };
            SDL.SDL_BlitSurface(textSurf, nint.Zero, box, ref dst);

            // texture
            nint tex = SDL.SDL_CreateTextureFromSurface(renderer, box);
            SDL.SDL_SetTextureBlendMode(tex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL.SDL_FreeSurface(box);

            return (tex,tw, th);
        }

     

        public static string CleanText(string text)
        {
            // normalize + drop trailing newlines (matches render)
            string s = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            int end = s.Length; 
            while (end > 0 && s[end - 1] == '\n') end--;
            var result = s.Substring(0, end);
            return result;
        }

        
        public static (int w, int h) MeasureSDLText(nint fontHandle, string text, uint wrapWidth = 0)
        {
            string s = text;

            var col = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
            nint surf = SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(fontHandle, s, col, 0);
            if (surf == nint.Zero) throw new Exception(SDL_ttf.TTF_GetError());

            var ss = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
            int w = ss.w, h = ss.h;
            SDL.SDL_FreeSurface(surf);
            return (w, h);
        }

        public static nint CreateEmptySurface(int width, int height)
        {
            var FMT = SDL.SDL_PIXELFORMAT_RGBA8888;
            nint surf = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, FMT);
            if (surf == nint.Zero) throw new Exception(SDL.SDL_GetError());
            SDL.SDL_FillRect(surf, nint.Zero, 0x00000000); // clear to transparent
            return surf;
        }


        /// <summary>Returns the bounding box (w,h) for multiline text (handles \n). No wrapping.</summary>
        private static (int w, int h) MeasureSDLTextOLD(nint fontHandle, string text)
        {
            var font = fontHandle;
            if (string.IsNullOrEmpty(text))
                return (0, SDL_ttf.TTF_FontHeight(font));

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int maxW = 0;
            int baseH = SDL_ttf.TTF_FontHeight(font);
            int lineSkip = SDL_ttf.TTF_FontLineSkip(font);

            foreach (var line in lines)
            {
                var s = line.Length == 0 ? " " : line; // empty line still has height
                SDL_ttf.TTF_SizeUTF8(font, s, out int w, out _);
                if (w > maxW) maxW = w;
            }
            int totalH = baseH + (lines.Length - 1) * lineSkip;

            return (200, 50);
        }
    }
}
