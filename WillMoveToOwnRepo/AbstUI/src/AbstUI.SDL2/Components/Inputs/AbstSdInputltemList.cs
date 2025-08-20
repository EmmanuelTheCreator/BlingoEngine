using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdInputltemList : AbstSdlScrollViewer, IAbstFrameworkItemList, ISdlFocusable, IDisposable
    {
        private bool _focused;
        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        private ISdlFontLoadedByUser? _font;
        private int _lineHeight;
        private int _hoverIndex = -1;
        private int _pressedIndex = -1;

        public bool Enabled { get; set; } = true;

        public string? Font { get; set; }
        public int FontSize { get; set; } = 11;
        public AColor TextColor { get; set; } = AbstDefaultColors.InputTextColor;

        public AColor ItemSelectedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
        public AColor ItemSelectedBGColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public AColor ItemSelectedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        public AColor ItemHoverTextColor { get; set; } = AbstDefaultColors.InputTextColor;
        public AColor ItemHoverBGColor { get; set; } = AbstDefaultColors.ListHoverColor;
        public AColor ItemHoverBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        public AColor ItemPressedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
        public AColor ItemPressedBGColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public AColor ItemPressedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public object FrameworkNode => this;


        public AbstSdInputltemList(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
            ComponentContext.QueueRedraw(this);
        }
        public void ClearItems()
        {
            _items.Clear();
            SelectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
            _hoverIndex = -1;
            _pressedIndex = -1;
            ComponentContext.QueueRedraw(this);
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

        protected override void RenderContent(AbstSDLRenderContext context)
        {
            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            int start = (int)MathF.Max(ScrollVertical / _lineHeight, 0);
            int y = -(int)(ScrollVertical - start * _lineHeight);
            for (int i = start; i < _items.Count && y < h; i++)
            {
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = y, w = w, h = _lineHeight };
                AColor bg = AbstDefaultColors.Input_Bg;
                AColor border = AbstDefaultColors.InputBorderColor;
                AColor txt = TextColor;
                bool isPressed = i == _pressedIndex;
                bool isSelected = i == SelectedIndex;
                bool isHover = i == _hoverIndex;
                if (isPressed)
                {
                    bg = ItemPressedBGColor;
                    border = ItemPressedBorderColor;
                    txt = ItemPressedTextColor;
                }
                else if (isSelected)
                {
                    bg = ItemSelectedBGColor;
                    border = ItemSelectedBorderColor;
                    txt = ItemSelectedTextColor;
                }
                else if (isHover)
                {
                    bg = ItemHoverBGColor;
                    border = ItemHoverBorderColor;
                    txt = ItemHoverTextColor;
                }

                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                DrawItemText(_items[i].Value, 4, y, new SDL.SDL_Color { r = txt.R, g = txt.G, b = txt.B, a = txt.A }, context);
                y += _lineHeight;
            }

            ContentWidth = w;
            ContentHeight = _lineHeight * _items.Count;
        }

        private void DrawItemText(string text, int x, int y, SDL.SDL_Color color, AbstSDLRenderContext ctx)
        {
            SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, text, out int tw, out int th);
            int baseline = y + _lineHeight - 4 - SDL_ttf.TTF_FontDescent(_font.FontHandle);
            int ty = baseline - SDL_ttf.TTF_FontAscent(_font.FontHandle);
            nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, text, color);
            nint textTex = SDL.SDL_CreateTextureFromSurface(ctx.Renderer, textSurf);
            SDL.SDL_FreeSurface(textSurf);
            SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            var dst = new SDL.SDL_Rect { x = x, y = ty, w = tw, h = th };
            SDL.SDL_RenderCopy(ctx.Renderer, textTex, IntPtr.Zero, ref dst);
            SDL.SDL_DestroyTexture(textTex);
        }

        protected override void HandleContentEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                if (ev.button.x >= X && ev.button.x <= X + Width &&
                    ev.button.y >= Y && ev.button.y <= Y + Height)
                {
                    Factory.FocusManager.SetFocus(this);
                    int idx = (int)((ev.button.y - Y + ScrollVertical) / _lineHeight);
                    if (idx >= 0 && idx < _items.Count)
                    {
                        _pressedIndex = idx;
                        SelectedIndex = idx;
                        SelectedKey = _items[idx].Key;
                        SelectedValue = _items[idx].Value;
                        ValueChanged?.Invoke();
                        ComponentContext.QueueRedraw(this);
                        e.StopPropagation = true;
                    }
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                if (_pressedIndex != -1)
                {
                    _pressedIndex = -1;
                    ComponentContext.QueueRedraw(this);
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                int newHover = -1;
                if (ev.motion.x >= X && ev.motion.x <= X + Width &&
                    ev.motion.y >= Y && ev.motion.y <= Y + Height)
                {
                    newHover = (int)((ev.motion.y - Y + ScrollVertical) / _lineHeight);
                    if (newHover < 0 || newHover >= _items.Count) newHover = -1;
                }
                if (newHover != _hoverIndex)
                {
                    _hoverIndex = newHover;
                    ComponentContext.QueueRedraw(this);
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
            _font?.Release();
            base.Dispose();
        }
    }
}
