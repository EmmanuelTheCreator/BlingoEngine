using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
using AbstUI.SDL2.Styles;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdInputltemList : AbstSdlComponent, IAbstFrameworkItemList, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        public AbstSdInputltemList(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public object FrameworkNode => this;
        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
        }
        public void ClearItems()
        {
            _items.Clear();
            SelectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
        }
        private bool _focused;
        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;
        private nint _texture;
        private int _texW;
        private int _texH;
        private int _lineHeight;

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, null, 12);
            _atlas ??= new SdlGlyphAtlas(ctx.Renderer, _font.FontHandle);
            if (_lineHeight == 0)
            {
                int ascent = SDL_ttf.TTF_FontAscent(_font.FontHandle);
                int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                _lineHeight = ascent - descent + 4;
            }
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            bool needRender = _texture == nint.Zero || _texW != w || _texH != h;
            if (needRender)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            }

            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
            SDL.SDL_RenderClear(context.Renderer);

            int y = 0;
            for (int i = 0; i < _items.Count && y < h; i++)
            {
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = y, w = w, h = _lineHeight };
                if (i == SelectedIndex)
                {
                    SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 120, 215, 255);
                    SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                    SDL.SDL_Color txt = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
                    DrawItemText(_items[i].Value, 4, y, txt, context);
                }
                else
                {
                    SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
                    SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                    SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                    SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                    SDL.SDL_Color txt = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
                    DrawItemText(_items[i].Value, 4, y, txt, context);
                }
                y += _lineHeight;
            }

            SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);
            _texW = w;
            _texH = h;
            return _texture;
        }

        private void DrawItemText(string text, int x, int y, SDL.SDL_Color color, AbstSDLRenderContext ctx)
        {
            List<int> cps = new();
            foreach (var r in text.EnumerateRunes()) cps.Add(r.Value);
            var span = CollectionsMarshal.AsSpan(cps);
            int baseline = y + _lineHeight - 4 - SDL_ttf.TTF_FontDescent(_font!.FontHandle);
            _atlas!.DrawRun(span, x, baseline, color);
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                if (ev.button.x >= X && ev.button.x <= X + Width &&
                    ev.button.y >= Y && ev.button.y <= Y + Height)
                {
                    Factory.FocusManager.SetFocus(this);
                    int idx = (int)((ev.button.y - Y) / _lineHeight);
                    if (idx >= 0 && idx < _items.Count)
                    {
                        SelectedIndex = idx;
                        SelectedKey = _items[idx].Key;
                        SelectedValue = _items[idx].Value;
                        ValueChanged?.Invoke();
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_KEYDOWN && _focused)
            {
                if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
                {
                    if (SelectedIndex > 0)
                    {
                        SelectedIndex--;
                        SelectedKey = _items[SelectedIndex].Key;
                        SelectedValue = _items[SelectedIndex].Value;
                        ValueChanged?.Invoke();
                        ComponentContext.QueueRedraw(this);
                    }
                    e.StopPropagation = true;
                }
                else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
                {
                    if (SelectedIndex < _items.Count - 1)
                    {
                        SelectedIndex++;
                        SelectedKey = _items[SelectedIndex].Key;
                        SelectedValue = _items[SelectedIndex].Value;
                        ValueChanged?.Invoke();
                        ComponentContext.QueueRedraw(this);
                    }
                    e.StopPropagation = true;
                }
            }
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            _font?.Release();
            _atlas?.Dispose();
            base.Dispose();
        }
    }
}
