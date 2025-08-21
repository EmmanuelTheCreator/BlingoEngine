using System;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlInputItemList : AbstSdlSelectableCollection, IAbstFrameworkItemList, ISdlFocusable, IDisposable
    {
        private bool _focused;
        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        private ISdlFontLoadedByUser? _font;
        private int _lineHeight;
        private int _hoverIndex = -1;
        private int _pressedIndex = -1;

        public AbstSdlInputItemList(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public override void ClearItems()
        {
            base.ClearItems();
            _hoverIndex = -1;
            _pressedIndex = -1;
        }

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, ItemFont, ItemFontSize);
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
            for (int i = start; i < Items.Count && y < h; i++)
            {
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = y, w = w, h = _lineHeight };
                AColor bg = AbstDefaultColors.Input_Bg;
                AColor border = AbstDefaultColors.InputBorderColor;
                AColor txt = ItemTextColor;
                bool isPressed = i == _pressedIndex;
                bool isSelected = i == SelectedIndex;
                bool isHover = i == _hoverIndex;
                if (isPressed)
                {
                    bg = ItemPressedBackgroundColor;
                    border = ItemPressedBorderColor;
                    txt = ItemPressedTextColor;
                }
                else if (isSelected)
                {
                    bg = ItemSelectedBackgroundColor;
                    border = ItemSelectedBorderColor;
                    txt = ItemSelectedTextColor;
                }
                else if (isHover)
                {
                    bg = ItemHoverBackgroundColor;
                    border = ItemHoverBorderColor;
                    txt = ItemHoverTextColor;
                }

                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                DrawItemText(Items[i].Value, 4, y, new SDL.SDL_Color { r = txt.R, g = txt.G, b = txt.B, a = txt.A }, context);
                y += _lineHeight;
            }

            ContentWidth = w;
            ContentHeight = _lineHeight * Items.Count;
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
                if (ev.button.x >= ComponentContext.X && ev.button.x <= ComponentContext.X + Width &&
                    ev.button.y >= ComponentContext.Y && ev.button.y <= ComponentContext.Y + Height)
                {
                    Factory.FocusManager.SetFocus(this);
                    int idx = (int)((ev.button.y - ComponentContext.Y + ScrollVertical) / _lineHeight);
                    if (idx >= 0 && idx < Items.Count)
                    {
                        _pressedIndex = idx;
                        SelectedIndex = idx;
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
                if (ev.motion.x >= ComponentContext.X && ev.motion.x <= ComponentContext.X + Width &&
                    ev.motion.y >= ComponentContext.Y && ev.motion.y <= ComponentContext.Y + Height)
                {
                    newHover = (int)((ev.motion.y - ComponentContext.Y + ScrollVertical) / _lineHeight);
                    if (newHover < 0 || newHover >= Items.Count) newHover = -1;
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
                        ComponentContext.QueueRedraw(this);
                    }
                    e.StopPropagation = true;
                }
                else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
                {
                    if (SelectedIndex < Items.Count - 1)
                    {
                        SelectedIndex++;
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
