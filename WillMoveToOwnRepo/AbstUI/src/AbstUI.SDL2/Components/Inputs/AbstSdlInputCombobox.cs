using System;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Components.Containers;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlInputCombobox : AbstSdlSelectableCollection, IAbstFrameworkInputCombobox, IHandleSdlEvent, ISdlFocusable, IDisposable, IHasTextBackgroundBorderColor
    {

        private AbstSdlInputItemList? _popup;
        private bool _open;
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private int _texW;
        private int _texH;
        private string _renderedText = string.Empty;
        private int _lineHeight;
        private bool _focused;
        public string? Font { get; set; }
        public int FontSize { get; set; } = 11;
        public AColor TextColor { get; set; } = AbstDefaultColors.InputTextColor;
        public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public new AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public bool HasFocus => _focused;


        public AbstSdlInputCombobox(AbstSdlComponentFactory factory) : base(factory)
        {
            Width = 80;
            Height = 20;
        }

        public override void AddItem(string key, string value)
        {
            base.AddItem(key, value);
            _popup?.AddItem(key, value);
        }

        public override void ClearItems()
        {
            base.ClearItems();
            _popup?.ClearItems();
        }

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, Font, FontSize);
            if (_lineHeight == 0)
            {
                int ascent = SDL_ttf.TTF_FontAscent(_font.FontHandle);
                int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                _lineHeight = ascent - descent + 4;
            }
        }

        public new void HandleEvent(AbstSDLEvent e)
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
                    if (SelectedIndex < Items.Count - 1)
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
                _popup = new AbstSdlInputItemList(Factory, ComponentContext);
                foreach (var it in Items)
                    _popup.AddItem(it.Key, it.Value);
                _popup.ValueChanged += PopupOnValueChanged;
            }

            _popup.ItemFont = ItemFont ?? Font;
            _popup.ItemFontSize = ItemFontSize;
            _popup.ItemTextColor = ItemTextColor;
            _popup.ItemSelectedTextColor = ItemSelectedTextColor;
            _popup.ItemSelectedBackgroundColor = ItemSelectedBackgroundColor;
            _popup.ItemSelectedBorderColor = ItemSelectedBorderColor;
            _popup.ItemHoverTextColor = ItemHoverTextColor;
            _popup.ItemHoverBackgroundColor = ItemHoverBackgroundColor;
            _popup.ItemHoverBorderColor = ItemHoverBorderColor;
            _popup.ItemPressedTextColor = ItemPressedTextColor;
            _popup.ItemPressedBackgroundColor = ItemPressedBackgroundColor;
            _popup.ItemPressedBorderColor = ItemPressedBorderColor;

            UpdatePopupPosition();
            _popup.Width = Width;
            int desired = Items.Count * _lineHeight + 2;
            _popup.Height = desired > 200 ? 200 : desired;
            _popup.Visibility = true;
            Factory.RootContext.ComponentContainer.Activate(_popup.ComponentContext);
            _open = true;
        }

        private void UpdatePopupPosition()
        {
            if (_popup == null) return;
            var (sx, sy) = GetScreenPosition();
            _popup.X = sx;
            _popup.Y = sy + Height;
        }

        private (int x, int y) GetScreenPosition()
        {
            int x = 0, y = 0;
            var ctx = ComponentContext;
            while (ctx != null)
            {
                x += ctx.X;
                y += ctx.Y;
                if (ctx.Component is AbstSdlScrollViewer sv)
                {
                    x -= (int)sv.ScrollHorizontal;
                    y -= (int)sv.ScrollVertical;
                }
                ctx = ctx.Parent;
            }
            return (x, y);
        }

        protected override void RenderContent(AbstSDLRenderContext context) { }

        protected override void HandleContentEvent(AbstSDLEvent e) { }

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
            ClosePopup();
            ComponentContext.QueueRedraw(this);
        }

        private bool HitTest(int x, int y) => x >= ComponentContext.X && x <= ComponentContext.X + Width && y >= ComponentContext.Y && y <= ComponentContext.Y + Height;


        public void SetFocus(bool focus)
        {
            _focused = focus;
            if (!focus) ClosePopup();
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (_open)
                UpdatePopupPosition();
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

                var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);
                SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
                SDL.SDL_RenderClear(context.Renderer);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_SetRenderDrawColor(context.Renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                if (!string.IsNullOrEmpty(text))
                {
                    int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                    int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                    int baseline = (h - (ascent - descent)) / 2 + ascent;
                    int ty = baseline - ascent;
                    var color = TextColor.ToSDLColor();
                    SDL_ttf.TTF_SizeUTF8(_font.FontHandle, text, out int tw, out int th);
                    nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, text, color);
                    nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                    SDL.SDL_FreeSurface(textSurf);
                    SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    var dst = new SDL.SDL_Rect { x = 4, y = ty, w = tw, h = th };
                    SDL.SDL_RenderCopy(context.Renderer, textTex, IntPtr.Zero, ref dst);
                    SDL.SDL_DestroyTexture(textTex);
                }

                SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
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
            _popup?.Dispose();
            base.Dispose();
        }
    }
}
