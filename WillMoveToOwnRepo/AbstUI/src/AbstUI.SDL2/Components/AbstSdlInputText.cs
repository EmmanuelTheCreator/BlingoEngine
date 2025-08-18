using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
using AbstUI.SDL2.Styles;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputText : AbstSdlComponent, IAbstFrameworkInputText, IHandleSdlEvent, IDisposable
    {
        private readonly bool _multiLine;
        private readonly List<int> _codepoints = new();
        private int _caret;
        private bool _focused;
        private uint _blinkStart;
        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;

        public AbstSdlInputText(AbstSdlComponentFactory factory, bool multiLine) : base(factory)
        {
            _multiLine = multiLine;
        }

        public bool Enabled { get; set; } = true;
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                _codepoints.Clear();
                foreach (var r in value.EnumerateRunes()) _codepoints.Add(r.Value);
                _caret = _codepoints.Count;
                ValueChanged?.Invoke();
            }
        }
        public int MaxLength { get; set; }
        public string? Font { get; set; }
        public int FontSize { get; set; } = 12;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;
        public AColor TextColor { get; set; } = AColors.Black;

        public bool IsMultiLine { get; set; }

        public bool HasFocus => _focused;

        public event Action? ValueChanged;

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            if (_font == null)
            {
                _font = ctx.SdlFontManager.GetTyped(this, Font, FontSize);
                _atlas = new SdlGlyphAtlas(ctx.Renderer, _font.FontHandle);
            }
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            ref var ev = ref e.Event;

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    _focused = true;
                    _caret = _codepoints.Count;
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    if (!_focused) break;
                    unsafe
                    {
                        fixed (byte* p = ev.text.text)
                        {
                            string s = SDL.UTF8_ToManaged((nint)p, false)!;
                            foreach (var r in s.EnumerateRunes())
                            {
                                if (MaxLength > 0 && _codepoints.Count >= MaxLength) break;
                                _codepoints.Insert(_caret, r.Value);
                                _caret++;
                            }
                        }
                    }
                    _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                    ValueChanged?.Invoke();
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (!_focused) break;
                    var key = ev.key.keysym.sym;
                    if (key == SDL.SDL_Keycode.SDLK_BACKSPACE)
                    {
                        if (_caret > 0)
                        {
                            _codepoints.RemoveAt(_caret - 1);
                            _caret--;
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                        }
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_DELETE)
                    {
                        if (_caret < _codepoints.Count)
                        {
                            _codepoints.RemoveAt(_caret);
                            _text = string.Concat(_codepoints.ConvertAll(cp => char.ConvertFromUtf32(cp)));
                            ValueChanged?.Invoke();
                        }
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_LEFT)
                    {
                        if (_caret > 0) _caret--;
                        e.StopPropagation = true;
                    }
                    else if (key == SDL.SDL_Keycode.SDLK_RIGHT)
                    {
                        if (_caret < _codepoints.Count) _caret++;
                        e.StopPropagation = true;
                    }
                    break;
            }
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(context);
            var renderer = context.Renderer;

            SDL.SDL_Rect rect = new SDL.SDL_Rect
            {
                x = (int)X,
                y = (int)Y,
                w = (int)Width,
                h = (int)Height
            };
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderFillRect(renderer, ref rect);
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderDrawRect(renderer, ref rect);

            if (_atlas == null || _font == null) return default;

            int ascent = SDL_ttf.TTF_FontAscent(_font.FontHandle);
            int baseline = (int)Y + (int)Height / 2 + ascent / 2;

            _atlas.DrawRun(CollectionsMarshal.AsSpan(_codepoints), (int)X + 4, baseline,
                TextColor.ToSDLColor());

            if (_focused)
            {
                uint ticks = SDL.SDL_GetTicks();
                if ((ticks - _blinkStart) > 1000) { _blinkStart = ticks; }
                if (((ticks - _blinkStart) / 500) % 2 == 0)
                {
                    int caretX = (int)X + 4 + _atlas.MeasureWidth(CollectionsMarshal.AsSpan(_codepoints).Slice(0, _caret));
                    SDL.SDL_RenderDrawLine(renderer, caretX, (int)Y + 2, caretX, (int)(Y + Height) - 2);
                }
            }

            return AbstSDLRenderResult.RequireRender();
        }

        public override void Dispose()
        {
            base.Dispose();
            _font?.Release();
            _atlas?.Dispose();
        }
    }
}
