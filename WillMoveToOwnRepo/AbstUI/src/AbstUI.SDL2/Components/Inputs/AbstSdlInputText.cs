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
        private bool _multiLine;
        private readonly List<int> _codepoints = new();
        private int _caret;
        private bool _focused;
        private uint _blinkStart;
        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;
        private int _scrollX;
        private int _scrollY;
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
        public virtual bool IsMultiLine { get => _multiLine; set => _multiLine = value; }

        public bool HasFocus => _focused;


        
        public event Action? ValueChanged;
        public AbstSdlInputText(AbstSdlComponentFactory factory, bool multiLine) : base(factory)
        {
            _multiLine = multiLine;
            Width = 80;
            Height = 20;
        }
        public override void Dispose()
        {
            base.Dispose();
            _font?.Release();
            _atlas?.Dispose();
        }
        private void EnsureResources(SdlFontManager fontManager, nint renderer)
        {
            if (_font == null)
            {
                _font = fontManager.GetTyped(this, Font, FontSize);
                _atlas = new SdlGlyphAtlas(renderer, _font.FontHandle);
            }
        }

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
            int lineHeight = SDL_ttf.TTF_FontHeight(_font.FontHandle) - (_multiLine ? 1 : 0);
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int innerX = (int)X + 4;
            int innerY = (int)Y + 2;
            int innerWidth = (int)Width - 8;
            int innerHeight = (int)Height - 4;
            SDL.SDL_Rect clip = new SDL.SDL_Rect
            {
                x = innerX,
                y = innerY,
                w = innerWidth,
                h = innerHeight
            };
            SDL.SDL_RenderSetClipRect(renderer, ref clip);

            int firstLineY;
            if (_multiLine)
            {
                firstLineY = innerY - _scrollY;
            }
            else
            {
                int baseline = (int)Y + ((int)Height + ascent + descent) / 2;
                firstLineY = baseline - ascent - _scrollY;
            }

            int drawX = innerX - _scrollX;
            int drawY = firstLineY;
            int index = 0;
            while (index <= span.Length)
            {
                int lineEnd = index;
                while (lineEnd < span.Length && span[lineEnd] != '\n') lineEnd++;
                var lineSpan = span.Slice(index, lineEnd - index);

                if (HasSelection)
                {
                    int selStart = Math.Min(_selectionStart, _caret);
                    int selEnd = Math.Max(_selectionStart, _caret);
                    int lineSelStart = Math.Max(selStart, index);
                    int lineSelEnd = Math.Min(selEnd, lineEnd);
                    if (lineSelStart < lineSelEnd)
                    {
                        int preWidth = _atlas.MeasureWidth(lineSpan.Slice(0, lineSelStart - index));
                        int selWidth = _atlas.MeasureWidth(lineSpan.Slice(lineSelStart - index, lineSelEnd - lineSelStart));
                        SDL.SDL_Rect selRect = new SDL.SDL_Rect
                        {
                            x = drawX + preWidth,
                            y = drawY,
                            w = selWidth,
                            h = lineHeight
                        };
                        SDL.SDL_SetRenderDrawColor(renderer, AbstDefaultColors.InputAccentColor.R, AbstDefaultColors.InputAccentColor.G, AbstDefaultColors.InputAccentColor.B, AbstDefaultColors.InputAccentColor.A);
                        SDL.SDL_RenderFillRect(renderer, ref selRect);

                        _atlas.DrawRun(lineSpan.Slice(0, lineSelStart - index), drawX, drawY + ascent, TextColor.ToSDLColor());
                        _atlas.DrawRun(lineSpan.Slice(lineSelStart - index, lineSelEnd - lineSelStart), drawX + preWidth, drawY + ascent, AbstDefaultColors.InputSelectionText.ToSDLColor());
                        _atlas.DrawRun(lineSpan.Slice(lineSelEnd - index), drawX + preWidth + selWidth, drawY + ascent, TextColor.ToSDLColor());
                    }
                    else
                    {
                        _atlas.DrawRun(lineSpan, drawX, drawY + ascent, TextColor.ToSDLColor());
                    }
                }
                else
                {
                    _atlas.DrawRun(lineSpan, drawX, drawY + ascent, TextColor.ToSDLColor());
                }

                if (lineEnd >= span.Length) break;
                index = lineEnd + 1;
                drawY += lineHeight;
            }

            if (_focused)
            {
                uint ticks = SDL.SDL_GetTicks();
                if (ticks - _blinkStart > 1000) { _blinkStart = ticks; }
                if ((ticks - _blinkStart) / 500 % 2 == 0)
                {
                    GetCaretPixel(out int cx, out int cy);
                    int drawCx = innerX - _scrollX + cx;
                    int drawCy = firstLineY + cy;
                    SDL.SDL_RenderDrawLine(renderer, drawCx, drawCy, drawCx, drawCy + lineHeight);
                }
            }

            SDL.SDL_RenderSetClipRect(renderer, nint.Zero);

            return AbstSDLRenderResult.RequireRender();
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
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    if (!_focused && !e.IsInside || !_multiLine) break;
                    // TODO : not working correctly, only depends on the carret, what is not de desired scroll effect
                    _scrollY += ev.wheel.y < 0 ? FontSize : -FontSize;
                    AdjustScroll();
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    Factory.FocusManager.SetFocus(this);
                    EnsureResources(Factory.FontManagerTyped, Factory.RootContext.Renderer);
                    int innerXDown = (int)X + 4;
                    int innerYDown = (int)Y + 2;
                    int clickX = (int)(e.ComponentLeft + _scrollX);
                    int clickY = (int)(e.ComponentTop + _scrollY);
                    _caret = GetCaretFromPixel(clickX, clickY);
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
                        int innerYMove = (int)Y + 2;
                        int x = (int)(e.ComponentLeft + _scrollX);
                        int y = (int)(e.ComponentTop + _scrollY);
                        _caret = GetCaretFromPixel(x, y);
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
                    HandleKeyDown(e, ev);
                    break;
            }
        }

        private void HandleKeyDown(AbstSDLEvent e, SDL.SDL_Event ev)
        {
            var key = ev.key.keysym.sym;
            SDL.SDL_Keymod mod = (SDL.SDL_Keymod)ev.key.keysym.mod;
            bool shift = (mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
            bool ctrl = (mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
            bool alt = (mod & SDL.SDL_Keymod.KMOD_ALT) != 0;
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
                _blinkStart = SDL.SDL_GetTicks();
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
                _blinkStart = SDL.SDL_GetTicks();
                AdjustScroll();
                e.StopPropagation = true;
            }
            else if (key == SDL.SDL_Keycode.SDLK_HOME)
            {
                if (shift && _selectionStart == -1)
                    _selectionStart = _caret;
                if (_multiLine)
                    _caret = GetLineStart(_caret);
                else
                    _caret = 0;
                if (!shift)
                    _selectionStart = -1;
                _blinkStart = SDL.SDL_GetTicks();
                AdjustScroll();
                e.StopPropagation = true;
            }
            else if (key == SDL.SDL_Keycode.SDLK_END)
            {
                if (shift && _selectionStart == -1)
                    _selectionStart = _caret;
                if (_multiLine)
                    _caret = GetLineEnd(_caret);
                else
                    _caret = _codepoints.Count;
                if (!shift)
                    _selectionStart = -1;
                _blinkStart = SDL.SDL_GetTicks();
                AdjustScroll();
                e.StopPropagation = true;
            }
            else if ((key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER) && _multiLine && !alt)
            {
                if (HasSelection)
                    DeleteSelection();
                if (MaxLength <= 0 || _codepoints.Count < MaxLength)
                {
                    _codepoints.Insert(_caret, '\n');
                    _caret++;
                    _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                    ValueChanged?.Invoke();
                    AdjustScroll();
                }
                e.StopPropagation = true;
            }
            else if (key == SDL.SDL_Keycode.SDLK_UP && _multiLine)
            {
                if (shift && _selectionStart == -1)
                    _selectionStart = _caret;
                MoveCaretVertical(-1);
                if (!shift)
                    _selectionStart = -1;
                AdjustScroll();
                e.StopPropagation = true;
            }
            else if (key == SDL.SDL_Keycode.SDLK_DOWN && _multiLine)
            {
                if (shift && _selectionStart == -1)
                    _selectionStart = _caret;
                MoveCaretVertical(1);
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
        }

        protected virtual bool IsAllowedText(string text) => TextValidate(text);

       
        protected virtual void AdjustScroll()
        {
            if (_atlas == null || _font == null) return;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            GetCaretPixel(out int caretX, out int caretY);
            int innerWidth = (int)Width - 8;
            int innerHeight = (int)Height - 4;
            int lineHeight = SDL_ttf.TTF_FontHeight(_font.FontHandle) - (_multiLine ? 1 : 0);
            if (caretX - _scrollX > innerWidth)
                _scrollX = caretX - innerWidth;
            else if (caretX - _scrollX < 0)
                _scrollX = caretX;
            if (caretY - _scrollY > innerHeight - lineHeight)
                _scrollY = caretY - (innerHeight - lineHeight);
            else if (caretY - _scrollY < 0)
                _scrollY = caretY;

            int maxWidth = 0;
            int lines = 1;
            int index = 0;
            while (index <= span.Length)
            {
                int lineEnd = index;
                while (lineEnd < span.Length && span[lineEnd] != '\n') lineEnd++;
                int w = _atlas.MeasureWidth(span.Slice(index, lineEnd - index));
                if (w > maxWidth) maxWidth = w;
                if (lineEnd >= span.Length) break;
                lines++;
                index = lineEnd + 1;
            }
            int textHeight = lines * lineHeight;
            int maxScrollX = Math.Max(0, maxWidth - innerWidth);
            int maxScrollY = Math.Max(0, textHeight - innerHeight);
            if (_scrollX > maxScrollX) _scrollX = maxScrollX;
            if (_scrollY > maxScrollY) _scrollY = maxScrollY;
            if (_scrollX < 0) _scrollX = 0;
            if (_scrollY < 0) _scrollY = 0;
        }

        protected virtual void DeleteSelection()
        {
            int start = Math.Min(_selectionStart, _caret);
            int end = Math.Max(_selectionStart, _caret);
            _codepoints.RemoveRange(start, end - start);
            _caret = start;
            _selectionStart = -1;
        }

        protected virtual int GetCaretFromPixel(int px, int py)
        {
            if (_atlas == null || _font == null) return _caret;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int lineHeight = SDL_ttf.TTF_FontHeight(_font.FontHandle) - (_multiLine ? 1 : 0);
            if (px <= 0 && py <= 0 && span.Length == 0) return 0;

            int lineIndex = py / lineHeight;
            if (lineIndex < 0) lineIndex = 0;
            int currentLine = 0;
            int index = 0;
            int lineStart = 0;
            while (index < span.Length && currentLine < lineIndex)
            {
                if (span[index] == '\n')
                {
                    currentLine++;
                    lineStart = index + 1;
                }
                index++;
            }
            int lineEnd = lineStart;
            while (lineEnd < span.Length && span[lineEnd] != '\n') lineEnd++;
            var lineSpan = span.Slice(lineStart, lineEnd - lineStart);
            int pos = 0;
            for (int i = 0; i < lineSpan.Length; i++)
            {
                int w = _atlas.MeasureWidth(lineSpan.Slice(i, 1));
                if (px < pos + w / 2)
                    return lineStart + i;
                pos += w;
            }
            return lineStart + lineSpan.Length;
        }

        protected virtual void GetCaretPixel(out int x, out int y)
        {
            x = 0;
            y = 0;
            if (_atlas == null || _font == null) return;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int lineHeight = SDL_ttf.TTF_FontHeight(_font.FontHandle) - (_multiLine ? 1 : 0);
            int lineStart = 0;
            int line = 0;
            for (int i = 0; i < _caret; i++)
            {
                if (span[i] == '\n')
                {
                    lineStart = i + 1;
                    line++;
                }
            }
            y = line * lineHeight;
            x = _atlas.MeasureWidth(span.Slice(lineStart, _caret - lineStart));
        }

        protected virtual int GetLineStart(int index)
        {
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int start = 0;
            for (int i = 0; i < index && i < span.Length; i++)
                if (span[i] == '\n') start = i + 1;
            return start;
        }

        protected virtual int GetLineEnd(int index)
        {
            var span = CollectionsMarshal.AsSpan(_codepoints);
            int i = index;
            while (i < span.Length && span[i] != '\n') i++;
            return i;
        }

        protected virtual void MoveCaretVertical(int lines)
        {
            if (_atlas == null || _font == null) return;
            var span = CollectionsMarshal.AsSpan(_codepoints);
            GetCaretPixel(out int caretX, out _);
            int targetLine = 0;
            int lineStart = 0;
            int line = 0;
            for (int i = 0; i < _caret; i++)
            {
                if (span[i] == '\n')
                {
                    line++;
                    lineStart = i + 1;
                }
            }
            targetLine = line + lines;
            if (targetLine < 0)
            {
                _caret = 0;
                return;
            }
            int index = 0;
            int currentLine = 0;
            int start = 0;
            while (index < span.Length && currentLine < targetLine)
            {
                if (span[index] == '\n')
                {
                    currentLine++;
                    start = index + 1;
                }
                index++;
            }
            if (currentLine < targetLine)
            {
                _caret = span.Length;
                return;
            }
            int end = start;
            while (end < span.Length && span[end] != '\n') end++;
            var lineSpan = span.Slice(start, end - start);
            int pos = 0;
            for (int i = 0; i < lineSpan.Length; i++)
            {
                int w = _atlas.MeasureWidth(lineSpan.Slice(i, 1));
                if (caretX < pos + w / 2)
                {
                    _caret = start + i;
                    return;
                }
                pos += w;
            }
            _caret = start + lineSpan.Length;
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
        public void SetFocus(bool focus)
        {
            _focused = focus;
            if (focus)
                _blinkStart = SDL.SDL_GetTicks();
            else
                _selectionStart = -1;
        }

        public void SetCaretPosition(int position)
        {
            _caret = Math.Clamp(position, 0, _codepoints.Count);
            _selectionStart = -1;
            AdjustScroll();
        }

        public void SetSelection(int start, int end)
        {
            _selectionStart = Math.Clamp(start, 0, _codepoints.Count);
            _caret = Math.Clamp(end, 0, _codepoints.Count);
            if (_selectionStart == _caret)
                _selectionStart = -1;
            AdjustScroll();
        }

        public void SetSelection(Range range)
        {
            SetSelection(range.Start.GetOffset(_codepoints.Count), range.End.GetOffset(_codepoints.Count));
        }

       
    }
}
