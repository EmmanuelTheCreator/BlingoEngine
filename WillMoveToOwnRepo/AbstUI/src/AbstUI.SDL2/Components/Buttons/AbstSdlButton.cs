using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Components.Buttons;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Buttons
{
    internal class AbstSdlButton : AbstSdlComponent, IAbstFrameworkButton, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private string _renderedText = string.Empty;
        private int _texW;
        private int _texH;
        private bool _pressed;
        private bool _focused;
        private readonly SdlFontManager _fontManager;
        private bool _isDirty = true;
        private string _text = string.Empty;
        private AColor _textColor = AColor.FromRGB(50, 50, 50);
        private AColor _borderColor = AColor.FromRGB(100, 100, 100);
        private AColor _backgroundColor = AColor.FromRGB(255, 255, 255);
        private AColor _backgroundHoverColor = AColor.FromRGB(200, 200, 200);
        private IAbstTexture2D? _iconTexture;
        private bool _isHover;

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value; _isDirty = true;
            }
        }
        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value; _isDirty = true;
            }
        }
        public AColor BackgroundHoverColor
        {
            get => _backgroundHoverColor;
            set
            {
                _backgroundHoverColor = value; _isDirty = true;
            }
        }
        public AColor TextColor { get => _textColor; set { _textColor = value; _isDirty = true; } }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get => _text; set { _text = value; _isDirty = true; } }
        public bool Enabled { get; set; } = true;
        public IAbstTexture2D? IconTexture
        {
            get => _iconTexture;
            set
            {
                _iconTexture = value;
                _isDirty = true;
            }
        }

        public object FrameworkNode => this;

        public event Action? Pressed;


        public AbstSdlButton(AbstSdlComponentFactory factory) : base(factory)
        {
            _fontManager = factory.FontManagerTyped;
            Width = 80;
            Height = 18;
        }
        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            if (_font == null)
                _font = _fontManager.GetDefaultFont<IAbstSdlFont>().Get(this, 12);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility || !_isDirty) return default;

            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            if (_texture == nint.Zero || _texW != w || _texH != h || _renderedText != Text)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                var color = _isHover ? _backgroundHoverColor : _backgroundColor;
                SDL.SDL_SetRenderDrawColor(context.Renderer, color.R, color.G, color.B, color.A);
                SDL.SDL_RenderClear(context.Renderer);
                SDL.SDL_SetRenderDrawColor(context.Renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                if (!string.IsNullOrEmpty(Text))
                {
                    
                    int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                    int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                    SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, Text, out int tw, out int th);
                    int baseline = (h - (SDL_ttf.TTF_FontAscent(_font.FontHandle) - SDL_ttf.TTF_FontDescent(_font.FontHandle))) / 2
                                 + SDL_ttf.TTF_FontAscent(_font.FontHandle);
                    int tx = (w - tw) / 2;
                    int ty = baseline - SDL_ttf.TTF_FontAscent(_font.FontHandle);

                    nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, Text, TextColor.ToSDLColor());
                    nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                    SDL.SDL_FreeSurface(textSurf);
                    SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    var dst = new SDL.SDL_Rect { x = tx, y = ty, w = tw, h = th };
                    SDL.SDL_RenderCopy(context.Renderer, textTex, IntPtr.Zero, ref dst);
                    SDL.SDL_DestroyTexture(textTex);
                }

                SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
                _renderedText = Text;
                _texW = w;
                _texH = h;
            }

            return _texture;
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;

            ref var ev = ref e.Event;
            if (!HitTest(ev.button.x, ev.button.y))
            {
                _isHover = false; 
                return;
            }
                switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        Factory.FocusManager.SetFocus(this);
                        _pressed = true;
                        e.StopPropagation = true;
                        _isDirty = true; 
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (_pressed && ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        Pressed?.Invoke();
                        _pressed = false;
                        e.StopPropagation = true;
                        _isDirty = true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    _isHover = true;
                    break;

            }
        }

        private bool HitTest(int mx, int my)
            => mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;

        public bool HasFocus => _focused;

        public void SetFocus(bool focus)
        {
            _focused = focus;
            _isDirty = true;
        }

        public void Invoke() => Pressed?.Invoke();

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _font?.Release();
            base.Dispose();
        }
    }
}
