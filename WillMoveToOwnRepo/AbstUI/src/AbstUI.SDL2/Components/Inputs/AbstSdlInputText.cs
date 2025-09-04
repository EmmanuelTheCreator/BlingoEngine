using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlInputText : AbstSdlComponent, IAbstFrameworkInputText, IFrameworkFor<AbstInputText>, IHandleSdlEvent, ISdlFocusable, IDisposable, IHasTextBackgroundBorderColor
    {
        private readonly bool _multiLine;
        private readonly List<int> _codepoints = new();
        private int _caret;
        private bool _focused;
        private uint _blinkStart;
        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;
        private int _scrollX;
        private int _selectionStart = -1;

        private bool HasSelection => _selectionStart != -1 && _selectionStart != _caret;


        public bool Enabled { get; set; } = true;
        private string _text = string.Empty;
        private bool _isHover;

        public virtual string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value ?? string.Empty;
                _codepoints.Clear();
                foreach (var r in value.EnumerateRunes()) _codepoints.Add(r.Value);
                _caret = _codepoints.Count;
                _selectionStart = -1;
                ValueChanged?.Invoke();
                AdjustScroll();
            }
        }
        public virtual int MaxLength { get; set; }
        public virtual string? Font { get; set; }
        public virtual int FontSize { get; set; } = 11;
        public virtual AMargin Margin { get; set; } = AMargin.Zero;
        public virtual object FrameworkNode => this;
        public virtual AColor TextColor { get; set; } = AbstDefaultColors.InputTextColor;
        public virtual AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public virtual AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        public Func<string, bool> TextValidate { get; set; } = s => true;
        public virtual bool IsMultiLine { get; set; }

        public bool HasFocus => _focused;


        public void SetFocus(bool focus)
        {
            _focused = focus;
            if (focus)
                _blinkStart = SDL.SDL_GetTicks();
            else
                _selectionStart = -1;
        }

        public event Action? ValueChanged;
        public AbstSdlInputText(AbstSdlComponentFactory factory, bool multiLine) : base(factory)
        {
            _multiLine = multiLine;
            Width = 80;
            Height = 20;
        }

        private void EnsureResources(SdlFontManager fontManager, nint renderer)
        {
            if (_font == null)
            {
                _font = fontManager.GetTyped(this, Font, FontSize);
                _atlas = new SdlGlyphAtlas(renderer, _font.FontHandle);
            }
        }
        public virtual bool CanHandleEvent(AbstSDLEvent e)
        {
            return Enabled && (e.IsInside || (_isHover && e.Event.type == SDL.SDL_EventType.SDL_MOUSEMOTION) || !e.HasCoordinates);
        }
        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            var ev = e.Event;

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    Factory.FocusManager.SetFocus(this);
                    EnsureResources(Factory.FontManagerTyped, Factory.RootContext.Renderer);
                    int innerXDown = (int)X + 4;
                    int clickX = ev.button.x - innerXDown + _scrollX;
                    _caret = GetCaretFromPixel(clickX);
                    _selectionStart = _caret;
                    AdjustScroll();
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    _isHover = e.IsInside;
                    if (!_focused) break;
                    if ((ev.motion.state & SDL.SDL_BUTTON_LMASK) != 0)
                    {
                        EnsureResources(Factory.FontManagerTyped, Factory.RootContext.Renderer);
                        int innerXMove = (int)X + 4;
                        int x = ev.motion.x - innerXMove + _scrollX;
                        _caret = GetCaretFromPixel(x);
                        AdjustScroll();
                        e.StopPropagation = true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    if (!_focused) break;
                    if (HasSelection)
                        DeleteSelection();
                    unsafe
                    {
                        fixed (byte* p = e.Event.text.text)
                        {
                            string s = SDL.UTF8_ToManaged((nint)p, false)!;
                            if (IsAllowedText(s))
                            {
                                foreach (var r in s.EnumerateRunes())
                                {
                                    if (MaxLength > 0 && _codepoints.Count >= MaxLength) break;
                                    _codepoints.Insert(_caret, r.Value);
                                    _caret++;
                                }
                            }
                        }
                    }
                    _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                    ValueChanged?.Invoke();
                    AdjustScroll();
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (!_focused) break;
                    var key = ev.key.keysym.sym;
                    SDL.SDL_Keymod mod = (SDL.SDL_Keymod)ev.key.keysym.mod;
                    bool shift = (mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
                    bool ctrl = (mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
                    if (key == SDL.SDL_Keycode.SDLK_BACKSPACE)
                    {
                        if (HasSelection)
                        {
                            DeleteSelection();
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                            AdjustScroll();
                        }
                        else if (_caret > 0)
                        {
                            _codepoints.RemoveAt(_caret - 1);
                            _caret--;
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                            AdjustScroll();
                        }
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_DELETE)
                    {
                        if (HasSelection)
                        {
                            DeleteSelection();
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                            AdjustScroll();
                        }
                        else if (_caret < _codepoints.Count)
                        {
                            _codepoints.RemoveAt(_caret);
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                            AdjustScroll();
                        }
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_LEFT)
                    {
                        if (shift && _selectionStart == -1)
                            _selectionStart = _caret;
                        if (ctrl)
                            MoveCaretPreviousWord();
                        else if (_caret > 0)
                            _caret--;
                        if (!shift)
                            _selectionStart = -1;
                        AdjustScroll();
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_RIGHT)
                    {
                        if (shift && _selectionStart == -1)
                            _selectionStart = _caret;
                        if (ctrl)
                            MoveCaretNextWord();
                        else if (_caret < _codepoints.Count)
                            _caret++;
                        if (!shift)
                            _selectionStart = -1;
                        AdjustScroll();
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_HOME)
                    {
                        if (shift && _selectionStart == -1)
                            _selectionStart = _caret;
                        _caret = 0;
                        if (!shift)
                            _selectionStart = -1;
                        AdjustScroll();
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_END)
                    {
                        if (shift && _selectionStart == -1)
                            _selectionStart = _caret;
                        _caret = _codepoints.Count;
                        if (!shift)
                            _selectionStart = -1;
                        AdjustScroll();
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_c && ctrl)
                    {
                        if (HasSelection)
                        {
                            int start = Math.Min(_selectionStart, _caret);
                            int end = Math.Max(_selectionStart, _caret);
                            string sel = string.Concat(_codepoints.GetRange(start, end - start).ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            SDL.SDL_SetClipboardText(sel);
                        }
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_v && ctrl)
                    {
                        string clip = SDL.SDL_HasClipboardText() == SDL.SDL_bool.SDL_TRUE ? SDL.SDL_GetClipboardText() : string.Empty;
                        if (!string.IsNullOrEmpty(clip))
                        {
                            if (HasSelection)
                                DeleteSelection();
                            if (IsAllowedText(clip))
                            {
                                foreach (var r in clip.EnumerateRunes())
                                {
                                    if (MaxLength > 0 && _codepoints.Count >= MaxLength) break;
                                    _codepoints.Insert(_caret, r.Value);
                                    _caret++;
                                }
                                _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                                ValueChanged?.Invoke();
                                AdjustScroll();
                            }
                        }
                        e.StopPropagation = true;
                    }
                    break;
            }
        }
        protected virtual bool IsAllowedText(string text) => TextValidate(text);

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(Factory.FontManagerTyped, Factory.RootContext.Renderer);
            var renderer = context.Renderer;

            SDL.SDL_Rect rect = new SDL.SDL_Rect
            {
                x = (int)X,
                y = (int)Y,
                w = (int)Width,
                h = (int)Height
            };
            SDL.SDL_SetRenderDrawColor(renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            SDL.SDL_RenderFillRect(renderer, ref rect);
            SDL.SDL_SetRenderDrawColor(renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
            SDL.SDL_RenderDrawRect(renderer, ref rect);

            if (_atlas == null || _font == null) return default;
            AdjustScroll();

            int ascent = SDL_ttf.TTF_FontAscent(_font.FontHandle);
            int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
            int baseline = (int)Y + ((int)Height + ascent + descent) / 2;

            var span = CollectionsMarshal.AsSpan(_codepoints);
            int innerX = (int)X + 4;
            int innerWidth = (int)Width - 8;
            SDL.SDL_Rect clip = new SDL.SDL_Rect
            {
                x = innerX,
                y = (int)Y + 2,
                w = innerWidth,
                h = (int)Height - 4
            };
            SDL.SDL_RenderSetClipRect(renderer, ref clip);

            int drawX = innerX - _scrollX;
            if (HasSelection)
            {
                int selStart = Math.Min(_selectionStart, _caret);
                int selEnd = Math.Max(_selectionStart, _caret);
                var length = selEnd - selStart;
                if (length >= 0)
                {
                    int preWidth = _atlas.MeasureWidth(span.Slice(0, selStart));
                    int selWidth = _atlas.MeasureWidth(span.Slice(selStart, length));
                    SDL.SDL_Rect selRect = new SDL.SDL_Rect
                    {
                        x = drawX + preWidth,
                        y = clip.y,
                        w = selWidth,
                        h = clip.h
                    };
                    SDL.SDL_SetRenderDrawColor(renderer, AbstDefaultColors.InputAccentColor.R, AbstDefaultColors.InputAccentColor.G, AbstDefaultColors.InputAccentColor.B, AbstDefaultColors.InputAccentColor.A);
                    SDL.SDL_RenderFillRect(renderer, ref selRect);

                    _atlas.DrawRun(span.Slice(0, selStart), drawX, baseline, TextColor.ToSDLColor());
                    _atlas.DrawRun(span.Slice(selStart, selEnd - selStart), drawX + preWidth, baseline, AbstDefaultColors.InputSelectionText.ToSDLColor());
                    _atlas.DrawRun(span.Slice(selEnd), drawX + preWidth + selWidth, baseline, TextColor.ToSDLColor());
                }
            }
            else
            {
                _atlas.DrawRun(span, drawX, baseline, TextColor.ToSDLColor());
            }

            if (_focused)
            {
                uint ticks = SDL.SDL_GetTicks();
                if (ticks - _blinkStart > 1000) { _blinkStart = ticks; }
                if ((ticks - _blinkStart) / 500 % 2 == 0)
                {
                    int caretX = innerX - _scrollX + _atlas.MeasureWidth(span.Slice(0, _caret));
                    SDL.SDL_RenderDrawLine(renderer, caretX, (int)Y + 2, caretX, (int)(Y + Height) - 2);
                }
            }

            SDL.SDL_RenderSetClipRect(renderer, nint.Zero);

            return AbstSDLRenderResult.RequireRender();
        }

        protected virtual void AdjustScroll()
        {
            if (_atlas == null) return;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int caretPixel = _atlas.MeasureWidth(span.Slice(0, _caret));
            int innerWidth = (int)Width - 8;
            if (caretPixel - _scrollX > innerWidth)
                _scrollX = caretPixel - innerWidth;
            else if (caretPixel - _scrollX < 0)
                _scrollX = caretPixel;
            int textWidth = _atlas.MeasureWidth(span);
            int maxScroll = Math.Max(0, textWidth - innerWidth);
            if (_scrollX > maxScroll) _scrollX = maxScroll;
            if (_scrollX < 0) _scrollX = 0;
        }

        protected virtual void DeleteSelection()
        {
            int start = Math.Min(_selectionStart, _caret);
            int end = Math.Max(_selectionStart, _caret);
            _codepoints.RemoveRange(start, end - start);
            _caret = start;
            _selectionStart = -1;
        }

        protected virtual int GetCaretFromPixel(int px)
        {
            if (_atlas == null) return _caret;
            if (px <= 0) return 0;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int pos = 0;
            for (int i = 0; i < span.Length; i++)
            {
                int w = _atlas.MeasureWidth(span.Slice(i, 1));
                if (px < pos + w / 2)
                    return i;
                pos += w;
            }
            return span.Length;
        }

        protected virtual bool IsWordChar(int cp)
        {
            var r = new Rune(cp);
            return Rune.IsLetterOrDigit(r) || r.Value == '_';
        }

        protected virtual void MoveCaretPreviousWord()
        {
            if (_caret == 0) return;
            int i = _caret - 1;
            while (i > 0 && !IsWordChar(_codepoints[i])) i--;
            while (i > 0 && IsWordChar(_codepoints[i - 1])) i--;
            _caret = i;
        }

        protected virtual void MoveCaretNextWord()
        {
            int i = _caret;
            int count = _codepoints.Count;
            while (i < count && IsWordChar(_codepoints[i])) i++;
            while (i < count && !IsWordChar(_codepoints[i])) i++;
            _caret = i;
        }

        public override void Dispose()
        {
            base.Dispose();
            _font?.Release();
            _atlas?.Dispose();
        }
    }
}
