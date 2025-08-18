using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
using AbstUI.SDL2.Styles;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputCombobox : AbstSdlComponent, IAbstFrameworkInputCombobox, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        public AbstSdlInputCombobox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;
        private bool _focused;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                if (value >= 0 && value < _items.Count)
                {
                    SelectedKey = _items[value].Key;
                    SelectedValue = _items[value].Value;
                }
                else
                {
                    SelectedKey = null;
                    SelectedValue = null;
                }
                ComponentContext.QueueRedraw(this);
                ValueChanged?.Invoke();
            }
        }
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public object FrameworkNode => this;

        private AbstSdInputltemList? _popup;
        private bool _open;
        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;
        private nint _texture;
        private int _texW;
        private int _texH;
        private string _renderedText = string.Empty;
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

        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
            _popup?.AddItem(key, value);
            ComponentContext.QueueRedraw(this);
        }

        public void ClearItems()
        {
            _items.Clear();
            _popup?.ClearItems();
            _selectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
            ComponentContext.QueueRedraw(this);
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                bool inside = HitTest(ev.button.x, ev.button.y);
                if (inside)
                {
                    Factory.FocusManager.SetFocus(this);
                    if (_open)
                        ClosePopup();
                    else
                        OpenPopup();
                    e.StopPropagation = true;
                }
                else if (_open && _popup is { } pop)
                {
                    if (!(ev.button.x >= pop.X && ev.button.x <= pop.X + pop.Width &&
                          ev.button.y >= pop.Y && ev.button.y <= pop.Y + pop.Height))
                    {
                        ClosePopup();
                    }
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_KEYDOWN && _focused)
            {
                if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
                {
                    if (SelectedIndex < _items.Count - 1)
                        SelectedIndex++;
                    e.StopPropagation = true;
                }
                else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
                {
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                    e.StopPropagation = true;
                }
            }
        }

        private void OpenPopup()
        {
            if (_popup == null)
            {
                _popup = new AbstSdInputltemList(Factory);
                foreach (var it in _items)
                    _popup.AddItem(it.Key, it.Value);
                _popup.ValueChanged += PopupOnValueChanged;
            }

            _popup.X = X;
            _popup.Y = Y + Height;
            _popup.Width = Width;
            int desired = _items.Count * _lineHeight + 2;
            _popup.Height = desired > 200 ? 200 : desired;
            _popup.Visibility = true;
            Factory.RootContext.ComponentContainer.Activate(_popup.ComponentContext);
            _open = true;
        }

        private void ClosePopup()
        {
            if (_popup == null) return;
            _popup.Visibility = false;
            Factory.RootContext.ComponentContainer.Deactivate(_popup.ComponentContext);
            _open = false;
        }

        private void PopupOnValueChanged()
        {
            if (_popup == null) return;
            SelectedIndex = _popup.SelectedIndex;
            SelectedKey = _popup.SelectedKey;
            SelectedValue = _popup.SelectedValue;
            ValueChanged?.Invoke();
            ClosePopup();
            ComponentContext.QueueRedraw(this);
        }

        private bool HitTest(int x, int y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus)
        {
            _focused = focus;
            if (!focus) ClosePopup();
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            string text = SelectedValue ?? string.Empty;
            if (_texture == nint.Zero || _texW != w || _texH != h || _renderedText != text)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                SDL.SDL_SetRenderTarget(context.Renderer, _texture);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
                SDL.SDL_RenderClear(context.Renderer);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                if (!string.IsNullOrEmpty(text))
                {
                    List<int> cps = new();
                    foreach (var r in text.EnumerateRunes()) cps.Add(r.Value);
                    var span = CollectionsMarshal.AsSpan(cps);
                    int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                    int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                    int baseline = (h - (ascent - descent)) / 2 + ascent;
                    _atlas!.DrawRun(span, 4, baseline, new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 });
                }

                SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);
                _texW = w;
                _texH = h;
                _renderedText = text;
            }

            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _font?.Release();
            _atlas?.Dispose();
            _popup?.Dispose();
            base.Dispose();
        }
    }
}
