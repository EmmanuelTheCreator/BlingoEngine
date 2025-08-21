using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
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
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private int _texW;
        private int _texH;
        private readonly List<SDL.SDL_Rect> _tabRects = new();
        private int _hoverIndex = -1;
        private int _tabHeight = 20;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public string? Font { get; set; }
        public int FontSize { get; set; } = 12;
        public AColor TextColor { get; set; } = AbstDefaultColors.Tab_Deselected_TextColor;

        public AColor SelectedTextColor { get; set; } = AbstDefaultColors.Tab_Selected_TextColor;

        public AColor TabHeaderBGColor { get; set; } = AbstDefaultColors.BG_Tabs;
        public AColor TabHeaderBorderColor { get; set; } = AbstDefaultColors.Border_Tabs;

        public AColor TabHeaderSelectedBGColor { get; set; } = AbstDefaultColors.BG_Tabs_Hover;
        public AColor TabHeaderSelectedBorderColor { get; set; } = AbstDefaultColors.TabActiveBorder;

        public AColor TabHeaderHoverBGColor { get; set; } = AbstDefaultColors.BG_Tabs_Hover;
        public AColor TabHeaderHoverBorderColor { get; set; } = AbstDefaultColors.TabActiveBorder;

        public AColor BackgroundColor { get; set; } = AbstDefaultColors.BG_WhiteMenus;
        public AColor BorderColor { get; set; } = AbstDefaultColors.Border_Tabs;
        public int BorderThickness { get; set; } = 1;


        public string SelectedTabName =>
            _selectedIndex >= 0 && _selectedIndex < _children.Count ? _children[_selectedIndex].Title : string.Empty;

        public bool HasFocus => _focused;

        public void SetFocus(bool focus) => _focused = focus;

        public AbstSdlTabContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public void AddTab(IAbstFrameworkTabItem content)
        {
            _children.Add(content);
            if (_selectedIndex == -1)
                _selectedIndex = 0;
            _texture = nint.Zero;
        }

        public void RemoveTab(IAbstFrameworkTabItem content)
        {
            var index = _children.IndexOf(content);
            if (index >= 0)
            {
                _children.RemoveAt(index);
                if (_selectedIndex >= _children.Count)
                    _selectedIndex = _children.Count - 1;
                _texture = nint.Zero;
            }
        }

        public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _children.ToArray();

        public void ClearTabs()
        {
            _children.Clear();
            _selectedIndex = -1;
            _texture = nint.Zero;
        }

        public void SelectTabByName(string tabName)
        {
            var idx = _children.FindIndex(t => t.Title == tabName);
            if (idx >= 0)
            {
                _selectedIndex = idx;
                _texture = nint.Zero;
            }
        }


        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, Font, FontSize);
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

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            SDL.SDL_RenderClear(context.Renderer);

            SDL.SDL_Rect contentRect = new SDL.SDL_Rect { x = 0, y = _tabHeight, w = w, h = h - _tabHeight };
            SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            SDL.SDL_RenderFillRect(context.Renderer, ref contentRect);
            if (BorderThickness > 0)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
                for (int t = 0; t < BorderThickness; t++)
                {
                    SDL.SDL_Rect br = new SDL.SDL_Rect
                    {
                        x = t,
                        y = _tabHeight + t,
                        w = w - 2 * t,
                        h = h - _tabHeight - 2 * t
                    };
                    SDL.SDL_RenderDrawRect(context.Renderer, ref br);
                }
            }

            _tabRects.Clear();
            int x = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                var tab = _children[i];
                int ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                int descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
                SDL_ttf.TTF_SizeUTF8(_font.FontHandle, tab.Title, out int textW, out int textH);
                int tw = textW + 10;
                int th = ascent - descent + 4;
                int tabW = Math.Max(tw, 60);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = x, y = 0, w = tabW, h = _tabHeight };
                _tabRects.Add(rect);
                AColor bg = TabHeaderBGColor;
                AColor border = TabHeaderBorderColor;
                AColor txtCol = TextColor;
                if (i == _selectedIndex)
                {
                    bg = TabHeaderSelectedBGColor;
                    border = TabHeaderSelectedBorderColor;
                    txtCol = SelectedTextColor;
                }
                else if (i == _hoverIndex)
                {
                    bg = TabHeaderHoverBGColor;
                    border = TabHeaderHoverBorderColor;
                }
                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                int baseline = rect.y + (_tabHeight - th) / 2 + ascent;
                int tx = rect.x + (tabW - textW) / 2;
                int ty = baseline - ascent;
                var sdlTxtCol = new SDL.SDL_Color { r = txtCol.R, g = txtCol.G, b = txtCol.B, a = txtCol.A };
                nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, tab.Title, sdlTxtCol);
                nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                SDL.SDL_FreeSurface(textSurf);
                SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                var dst = new SDL.SDL_Rect { x = tx, y = ty, w = textW, h = textH };
                SDL.SDL_RenderCopy(context.Renderer, textTex, IntPtr.Zero, ref dst);
                SDL.SDL_DestroyTexture(textTex);
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
                    ctx.OffsetX += BorderThickness;
                    ctx.OffsetY += _tabHeight + BorderThickness;
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
            int localX;
            int localY;
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN when ev.button.button == SDL.SDL_BUTTON_LEFT:
                    localX = ev.button.x - ComponentContext.X;
                    localY = ev.button.y - ComponentContext.Y;
                    for (int i = 0; i < _tabRects.Count; i++)
                    {
                        var r = _tabRects[i];
                        if (localX >= r.x && localX <= r.x + r.w &&
                            localY >= r.y && localY <= r.y + r.h)
                        {
                            if (_selectedIndex != i)
                            {
                                _selectedIndex = i;
                                _texture = nint.Zero;
                                ComponentContext.QueueRedraw(this);
                            }
                            Factory.FocusManager.SetFocus(this);
                            e.StopPropagation = true;
                            break;
                        }
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    localX = ev.motion.x - ComponentContext.X;
                    localY = ev.motion.y - ComponentContext.Y;
                    int newHover = -1;
                    for (int i = 0; i < _tabRects.Count; i++)
                    {
                        var r = _tabRects[i];
                        if (localX >= r.x && localX <= r.x + r.w &&
                            localY >= r.y && localY <= r.y + r.h)
                        {
                            newHover = i;
                            break;
                        }
                    }
                    if (newHover != _hoverIndex)
                    {
                        _hoverIndex = newHover;
                        _texture = nint.Zero;
                        ComponentContext.QueueRedraw(this);
                    }
                    break;
            }

            // Forward mouse events to children accounting for current scroll offset

            for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            {
                if (_children[i].FrameworkNode is not AbstSdlComponent comp ||
                    comp is not IHandleSdlEvent handler ||
                    !comp.Visibility)
                    continue;
                ContainerHelpers.HandleChildEvents(comp, e, -(int)(ComponentContext.X + BorderThickness), -(int)(ComponentContext.Y + _tabHeight + BorderThickness));
            }
        }




        public override void Dispose()
        {
            ClearTabs();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            _font?.Release();
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
