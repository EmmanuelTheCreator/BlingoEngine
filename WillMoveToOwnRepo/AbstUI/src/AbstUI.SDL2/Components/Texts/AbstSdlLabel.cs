using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Texts;
using System.Runtime.InteropServices;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Texts
{
    public class AbstSdlLabel : AbstSdlComponent, IAbstFrameworkLabel, IFrameworkFor<AbstLabel>, IDisposable
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
            if (!Visibility || string.IsNullOrEmpty(Text))
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
                var (tex, textW, textH) = CreateTextTextureBox(context.Renderer, _font!.FontHandle, Text, (int)boxW, (int)boxH, TextAlignment, col);
                _texture = tex;
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                //SDL.SDL_FreeSurface(tex); // this breaks the texture, so we need keep it.

                // update sizes
                if (Width <= 0) Width = textW;
                if (Height <= 0) Height = textH;

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
        public static (nint tex, int textW, int textH) CreateTextTextureBox(nint renderer, nint fontHandle, string text, int boxW, int boxH, AbstTextAlignment align, SDL.SDL_Color col, bool wordWrap = true)
        {
            // --- local helper: wrap to width using TTF_MeasureUTF8
            static List<string> WrapLines(nint font, string t, int maxWidth)
            {
                var lines = new List<string>();
                foreach (var raw in t.Replace("\r", "").Split('\n'))
                {
                    var s = raw;
                    if (string.IsNullOrEmpty(s)) { lines.Add(string.Empty); continue; }

                    while (s.Length > 0)
                    {
                        SDL_ttf.TTF_MeasureUTF8(font, s, maxWidth, out int extent, out int count);
                        if (count <= 0 || count >= s.Length) { lines.Add(s); break; }

                        int cut = s.LastIndexOf(' ', Math.Clamp(count - 1, 0, s.Length - 1), count);
                        if (cut <= 0) cut = count;
                        lines.Add(s[..cut].TrimEnd());
                        s = s[cut..].TrimStart();
                    }
                }
                return lines;
            }

            // choose line list
            var hasLineBreak = text.IndexOf('\n') >= 0;
            List<string> lines;
            if (wordWrap || hasLineBreak)
            {
                int wrapWidth = boxW > 0 ? boxW : int.MaxValue / 4;
                lines = WrapLines(fontHandle, text, wrapWidth);
            }
            else
            {
                lines = new List<string> { text };
            }

            // measure
            int lineSkip = SDL_ttf.TTF_FontLineSkip(fontHandle);
            int maxLineW = 0;
            int maxLineH = 0;
            foreach (var ln in lines)
            {
                SDL_ttf.TTF_SizeUTF8(fontHandle, ln, out int lw, out int lh);
                if (lw > maxLineW) maxLineW = lw;
                if (lh > maxLineH) maxLineH = lh;
            }
            int totalH = Math.Max(lineSkip * Math.Max(1, lines.Count), maxLineH);
            int totalW = Math.Max(1, maxLineW);

            // fallback box size
            if (boxW <= 0) boxW = totalW;
            if (boxH <= 0) boxH = totalH;

            // target surface
            var FMT = SDL.SDL_PIXELFORMAT_RGBA8888;
            nint box = SDL.SDL_CreateRGBSurfaceWithFormat(0, boxW, boxH, 32, FMT);
            if (box == nint.Zero) throw new Exception(SDL.SDL_GetError());
            SDL.SDL_FillRect(box, nint.Zero, 0x00000000); // Transparent background
                                                          // SDL.SDL_FillRect(box, nint.Zero, 0xFFFFFFFF); // DEBUG white bg

            // vertical start (centered)
            int startY = Math.Max(0, (boxH - totalH) / 2);

            // optional small baseline tweak (keep from your code)
            int ascent = SDL_ttf.TTF_FontAscent(fontHandle);
            int fontHeight = SDL_ttf.TTF_FontHeight(fontHandle);
            int tightHeight = fontHeight - ascent;
            int sdlFixY = tightHeight - 1;

            // draw lines
            int y = startY - sdlFixY;
            foreach (var ln in lines)
            {
                nint lineSurf = SDL_ttf.TTF_RenderUTF8_Blended(fontHandle, ln, col);
                if (lineSurf == nint.Zero) { SDL.SDL_FreeSurface(box); throw new Exception(SDL_ttf.TTF_GetError()); }
                var ls = Marshal.PtrToStructure<SDL.SDL_Surface>(lineSurf);
                int lw = ls.w, lh = ls.h;

                int x = align switch
                {
                    AbstTextAlignment.Center => Math.Max(0, (boxW - lw) / 2),
                    AbstTextAlignment.Right => Math.Max(0, boxW - lw),
                    _ => 0
                };

                var dst = new SDL.SDL_Rect { x = x, y = y, w = lw, h = lh };
                SDL.SDL_BlitSurface(lineSurf, nint.Zero, box, ref dst);
                SDL.SDL_FreeSurface(lineSurf);

                y += lineSkip;
            }

            // texture
            nint tex = SDL.SDL_CreateTextureFromSurface(renderer, box);
            if (tex == nint.Zero) { SDL.SDL_FreeSurface(box); throw new Exception(SDL.SDL_GetError()); }
            SDL.SDL_SetTextureBlendMode(tex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL.SDL_FreeSurface(box);
            return (tex, totalW, totalH);
        }



        /// Splits `text` into lines that fit `maxWidth` using word boundaries.
        private static List<string> WrapLines(nint font, string text, int maxWidth)
        {
            var lines = new List<string>();
            foreach (var raw in text.Replace("\r", "").Split('\n'))
            {
                var s = raw;
                while (s.Length > 0)
                {
                    SDL_ttf.TTF_MeasureUTF8(font, s, maxWidth, out int extent, out int count);
                    if (count <= 0 || count >= s.Length) { lines.Add(s); break; }
                    // back off to last space for cleaner wrap
                    int cut = s.LastIndexOf(' ', count - 1, count);
                    if (cut <= 0) cut = count;
                    lines.Add(s[..cut].TrimEnd());
                    s = s[cut..].TrimStart();
                }
                if (raw.Length == 0) lines.Add(string.Empty);
            }
            return lines;
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
