using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Components.Buttons;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;

namespace AbstUI.SDL2.Components.Buttons
{
    internal class AbstSdlStateButton : AbstSdlComponent, IAbstFrameworkStateButton, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        private ISdlFontLoadedByUser? _font;
        private readonly SdlFontManager _fontManager;
        private nint _texture;
        private nint _textureOnPtr;
        private nint _textureOffPtr;
        private int _texW;
        private int _texH;
        private bool _pressed;
        private bool _hover;
        private bool _focused;
        private bool _renderedState;
        private string _renderedText = string.Empty;
        private bool _isDirty = true;

        private IAbstTexture2D? _textureOn;
        private IAbstTexture2D? _textureOff;
        private string _text = string.Empty;
        private bool _isOn;

        private AColor _textColor = AColor.FromRGB(50, 50, 50);
        private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
        private AColor _borderHoverColor = AbstDefaultColors.Button_Border_Hover;
        private AColor _borderPressedColor = AbstDefaultColors.Button_Border_Pressed;
        private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
        private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
        private AColor _backgroundPressedColor = AbstDefaultColors.Button_Bg_Pressed;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;

        public bool Enabled { get; set; } = true;
        public string Text { get => _text; set { _text = value; _isDirty = true; } }
        public AColor BorderColor { get => _borderColor; set { _borderColor = value; _isDirty = true; } }
        public AColor BorderHoverColor { get => _borderHoverColor; set { _borderHoverColor = value; _isDirty = true; } }
        public AColor BorderPressedColor { get => _borderPressedColor; set { _borderPressedColor = value; _isDirty = true; } }
        public AColor BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; _isDirty = true; } }
        public AColor BackgroundHoverColor { get => _backgroundHoverColor; set { _backgroundHoverColor = value; _isDirty = true; } }
        public AColor BackgroundPressedColor { get => _backgroundPressedColor; set { _backgroundPressedColor = value; _isDirty = true; } }
        public AColor TextColor { get => _textColor; set { _textColor = value; _isDirty = true; } }

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
                _isDirty = true;
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
                _isDirty = true;
            }
        }

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    _isDirty = true;
                    ValueChanged?.Invoke();
                }
            }
        }

        public AbstSdlStateButton(AbstSdlComponentFactory factory) : base(factory)
        {
            Width = 20;
            Height = 20;
            _fontManager = factory.FontManagerTyped;
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

            if (_texture == nint.Zero || w != _texW || h != _texH)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
            }

            var prev = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);

            var bg = (_pressed || _isOn) ? _backgroundPressedColor : _hover ? _backgroundHoverColor : _backgroundColor;
            SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
            SDL.SDL_RenderClear(context.Renderer);

            int tw = 0, th = 0, tx = 0, ty = 0;
            if (!string.IsNullOrEmpty(_text))
            {
                SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, _text, out tw, out th);
                int baseline = (h - (SDL_ttf.TTF_FontAscent(_font.FontHandle) - SDL_ttf.TTF_FontDescent(_font.FontHandle))) / 2
                               + SDL_ttf.TTF_FontAscent(_font.FontHandle);
                ty = baseline - SDL_ttf.TTF_FontAscent(_font.FontHandle);
            }

            nint icon = _isOn ? (_textureOnPtr != nint.Zero ? _textureOnPtr : _textureOffPtr)
                              : (_textureOffPtr != nint.Zero ? _textureOffPtr : _textureOnPtr);

            if (icon != nint.Zero && !string.IsNullOrEmpty(_text))
            {
                SDL.SDL_QueryTexture(icon, out _, out _, out int iw, out int ih);
                int totalW = iw + 4 + tw;
                int startX = (w - totalW) / 2;
                SDL.SDL_Rect idst = new SDL.SDL_Rect { x = startX, y = (h - ih) / 2, w = iw, h = ih };
                SDL.SDL_RenderCopy(context.Renderer, icon, nint.Zero, ref idst);
                tx = startX + iw + 4;
            }
            else if (icon != nint.Zero)
            {
                SDL.SDL_QueryTexture(icon, out _, out _, out int iw, out int ih);
                SDL.SDL_Rect idst = new SDL.SDL_Rect { x = (w - iw) / 2, y = (h - ih) / 2, w = iw, h = ih };
                SDL.SDL_RenderCopy(context.Renderer, icon, nint.Zero, ref idst);
            }
            else if (!string.IsNullOrEmpty(_text))
            {
                tx = (w - tw) / 2;
            }

            if (!string.IsNullOrEmpty(_text))
            {
                nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, _text, _textColor.ToSDLColor());
                nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                SDL.SDL_FreeSurface(textSurf);
                SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL.SDL_Rect tdst = new SDL.SDL_Rect { x = tx, y = ty, w = tw, h = th };
                SDL.SDL_RenderCopy(context.Renderer, textTex, nint.Zero, ref tdst);
                SDL.SDL_DestroyTexture(textTex);
            }

            var border = (_pressed || _isOn) ? _borderPressedColor : _hover ? _borderHoverColor : _borderColor;
            SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
            SDL.SDL_Rect borderRect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
            SDL.SDL_RenderDrawRect(context.Renderer, ref borderRect);

            SDL.SDL_SetRenderTarget(context.Renderer, prev);
            _renderedText = _text;
            _renderedState = _isOn;
            _isDirty = false;
            return _texture;
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (!HitTest(ev.button.x, ev.button.y))
            {
                if (_hover)
                {
                    _hover = false;
                    _isDirty = true;
                }
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
                        IsOn = !IsOn;
                        _pressed = false;
                        e.StopPropagation = true;
                        _isDirty = true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    if (!_hover)
                    {
                        _hover = true;
                        _isDirty = true;
                    }
                    break;
            }
        }

        private bool HitTest(int mx, int my) => mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

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
            _font?.Release();
            base.Dispose();
        }
    }
}

