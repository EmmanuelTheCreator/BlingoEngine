using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Texts;
using AbstUI.SDL2.Styles;
using AbstUI.Components.Containers;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Containers
{
    public class AbstSdlTabContainer : AbstSdlComponent, IAbstFrameworkTabContainer, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        private readonly List<IAbstFrameworkTabItem> _children = new();
        private int _selectedIndex = -1;
        private bool _focused;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstSdlTabContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public string SelectedTabName =>
            _selectedIndex >= 0 && _selectedIndex < _children.Count ? _children[_selectedIndex].Title : string.Empty;

        public void AddTab(IAbstFrameworkTabItem content)
        {
            _children.Add(content);
            if (_selectedIndex == -1)
                _selectedIndex = 0;
        }

        public void RemoveTab(IAbstFrameworkTabItem content)
        {
            var index = _children.IndexOf(content);
            if (index >= 0)
            {
                _children.RemoveAt(index);
                if (_selectedIndex >= _children.Count)
                    _selectedIndex = _children.Count - 1;
            }
        }

        public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _children.ToArray();

        public void ClearTabs()
        {
            _children.Clear();
            _selectedIndex = -1;
        }

        public void SelectTabByName(string tabName)
        {
            var idx = _children.FindIndex(t => t.Title == tabName);
            if (idx >= 0)
                _selectedIndex = idx;
        }

        private ISdlFontLoadedByUser? _font;
        private SdlGlyphAtlas? _atlas;
        private nint _texture;
        private int _texW;
        private int _texH;
        private readonly List<SDL.SDL_Rect> _tabRects = new();

        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, null, 12);
            _atlas ??= new SdlGlyphAtlas(ctx.Renderer, _font.FontHandle);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            int tabHeight = 20;

            bool needRender = _texture == nint.Zero || _texW != w || _texH != h;
            if (needRender)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            }

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
            SDL.SDL_RenderClear(context.Renderer);

            _tabRects.Clear();
            int x = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                var tab = _children[i];
                List<int> cps = new();
                foreach (var r in tab.Title.EnumerateRunes()) cps.Add(r.Value);
                var span = CollectionsMarshal.AsSpan(cps);
                int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                int tw = _atlas!.MeasureWidth(span) + 10;
                int th = ascent - descent + 4;
                int tabW = Math.Max(tw, 60);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = x, y = 0, w = tabW, h = tabHeight };
                _tabRects.Add(rect);
                if (i == _selectedIndex)
                    SDL.SDL_SetRenderDrawColor(context.Renderer, 220, 220, 220, 255);
                else
                    SDL.SDL_SetRenderDrawColor(context.Renderer, 180, 180, 180, 255);
                SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                int baseline = rect.y + (tabHeight - th) / 2 + ascent;
                int tx = rect.x + (tabW - (tw - 10)) / 2;
                _atlas.DrawRun(span, tx, baseline, new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 });
                x += tabW;
            }

            if (_selectedIndex >= 0 && _selectedIndex < _children.Count)
            {
                var tab = _children[_selectedIndex];
                if (tab.Content?.FrameworkObj.FrameworkNode is AbstSdlComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += -X;
                    ctx.OffsetY += -Y - tabHeight;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }

            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            _texW = w;
            _texH = h;
            return _texture;
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            ref var ev = ref e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                for (int i = 0; i < _tabRects.Count; i++)
                {
                    var r = _tabRects[i];
                    if (ev.button.x >= r.x && ev.button.x <= r.x + r.w &&
                        ev.button.y >= r.y && ev.button.y <= r.y + r.h)
                    {
                        _selectedIndex = i;
                        Factory.FocusManager.SetFocus(this);
                        e.StopPropagation = true;
                        break;
                    }
                }
            }
        }

        public bool HasFocus => _focused;

        public void SetFocus(bool focus) => _focused = focus;

        public override void Dispose()
        {
            ClearTabs();
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

    public class AbstSdlTabItem : AbstSdlComponent, IAbstFrameworkTabItem
    {
        public AbstSdlTabItem(AbstSdlComponentFactory factory, AbstTabItem tab) : base(factory)
        {
            tab.Init(this);
        }

        public string Title { get; set; } = string.Empty;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public IAbstNode? Content { get; set; }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public override void Dispose() => base.Dispose();

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context) => default;
    }
}
