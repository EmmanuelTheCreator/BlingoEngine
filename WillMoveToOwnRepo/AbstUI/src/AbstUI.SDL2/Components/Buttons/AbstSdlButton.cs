using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
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
        private SdlGlyphAtlas? _atlas;
        private nint _texture;
        private string _renderedText = string.Empty;
        private int _texW;
        private int _texH;
        private bool _pressed;
        private bool _focused;
        private readonly SdlFontManager _fontManager;


        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public IAbstTexture2D? IconTexture { get; set; }

        public object FrameworkNode => this;

        public event Action? Pressed;


        public AbstSdlButton(AbstSdlComponentFactory factory) : base(factory)
        {
            _fontManager = factory.FontManagerTyped;
        }
        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font = _fontManager.GetDefaultFont<IAbstSdlFont>().Get(this, 12);
            _atlas ??= new SdlGlyphAtlas(ctx.Renderer, _font!.FontHandle);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

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

                SDL.SDL_SetRenderDrawColor(context.Renderer, 200, 200, 200, 255);
                SDL.SDL_RenderClear(context.Renderer);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                if (!string.IsNullOrEmpty(Text))
                {
                    List<int> cps = new();
                    foreach (var r in Text.EnumerateRunes()) cps.Add(r.Value);
                    var span = CollectionsMarshal.AsSpan(cps);
                    int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                    int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                    int tw = _atlas!.MeasureWidth(span);
                    int th = ascent - descent;
                    int baseline = (h - th) / 2 + ascent;
                    int tx = (w - tw) / 2;
                    _atlas.DrawRun(span, tx, baseline, new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 });
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
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT && HitTest(ev.button.x, ev.button.y))
                    {
                        Factory.FocusManager.SetFocus(this);
                        _pressed = true;
                        e.StopPropagation = true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (_pressed && ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        if (HitTest(ev.button.x, ev.button.y))
                            Pressed?.Invoke();
                        _pressed = false;
                        e.StopPropagation = true;
                    }
                    break;
            }
        }

        private bool HitTest(int mx, int my)
            => mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;

        public bool HasFocus => _focused;

        public void SetFocus(bool focus) => _focused = focus;

        public void Invoke() => Pressed?.Invoke();

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _font?.Release();
            _atlas?.Dispose();
            base.Dispose();
        }
    }
}
