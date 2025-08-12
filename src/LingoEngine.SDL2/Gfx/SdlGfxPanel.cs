using System;
using System.Collections.Generic;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxPanel : SdlGfxComponent, ILingoFrameworkGfxPanel, IDisposable
    {
        public SdlGfxPanel(SdlGfxFactory factory) : base(factory)
        {
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public LingoColor? BackgroundColor { get; set; }
        public LingoColor? BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public object FrameworkNode => this;

        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _children.ToArray();

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            _children.Remove(child);
        }

        public void RemoveAll()
        {
            _children.Clear();
        }

        private nint _texture;
        private int _texW;
        private int _texH;

        public override nint Render(LingoSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            int w = (int)Width;
            int h = (int)Height;
            if (_texture == nint.Zero || w != _texW || h != _texH)
            {
                if (_texture != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_texture);
                }
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                _texW = w;
                _texH = h;
            }

            SDL.SDL_SetRenderTarget(context.Renderer, _texture);

            if (BackgroundColor is { } bg)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderClear(context.Renderer);
            }
            else
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);
            }

            foreach (var child in _children)
            {
                if (child.FrameworkNode is SdlGfxComponent comp)
                {
                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += -X;
                    ctx.OffsetY += -Y;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }

            if (BorderWidth > 0 && BorderColor is { } bc)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, bc.R, bc.G, bc.B, bc.A);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                for (int i = 0; i < (int)BorderWidth; i++)
                {
                    SDL.SDL_RenderDrawRect(context.Renderer, ref rect);
                    rect.x++; rect.y++; rect.w -= 2; rect.h -= 2;
                }
            }

            SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);
            return _texture;
        }

        public override void Dispose()
        {
            RemoveAll();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }
    }
}
