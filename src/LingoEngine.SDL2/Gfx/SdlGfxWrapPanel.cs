using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxWrapPanel : SdlGfxComponent, IAbstUIFrameworkGfxWrapPanel, IDisposable
    {
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstUIFrameworkGfxLayoutNode> _children = new();

        public SdlGfxWrapPanel(SdlGfxFactory factory, AOrientation orientation) : base(factory)
        {
            Orientation = orientation;
            ItemMargin = new APoint(0, 0);
            Margin = AMargin.Zero;
        }

        public void AddItem(IAbstUIFrameworkGfxNode child)
        {
            if (child is IAbstUIFrameworkGfxLayoutNode layout && !_children.Contains(layout))
                _children.Add(layout);
        }

        public void RemoveItem(IAbstUIFrameworkGfxNode child)
        {
            if (child is IAbstUIFrameworkGfxLayoutNode layout)
                _children.Remove(layout);
        }

        public IEnumerable<IAbstUIFrameworkGfxNode> GetItems() => _children.ToArray();

        public IAbstUIFrameworkGfxNode? GetItem(int index)
        {
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index];
        }

        public void RemoveAll()
        {
            _children.Clear();
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

        private nint _texture;
        private int _texW;
        private int _texH;

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
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
            SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(context.Renderer);

            float curX = 0;
            float curY = 0;
            float lineSize = 0;
            foreach (var child in _children)
            {
                var margin = child.Margin;
                float childW = child.Width + margin.Left + margin.Right;
                float childH = child.Height + margin.Top + margin.Bottom;

                if (Orientation == AOrientation.Horizontal)
                {
                    if (curX + childW > Width)
                    {
                        curX = 0;
                        curY += lineSize + ItemMargin.Y;
                        lineSize = 0;
                    }
                    child.X = curX + margin.Left;
                    child.Y = curY + margin.Top;
                    curX += childW + ItemMargin.X;
                    lineSize = Math.Max(lineSize, childH);
                }
                else
                {
                    if (curY + childH > Height)
                    {
                        curY = 0;
                        curX += lineSize + ItemMargin.X;
                        lineSize = 0;
                    }
                    child.X = curX + margin.Left;
                    child.Y = curY + margin.Top;
                    curY += childH + ItemMargin.Y;
                    lineSize = Math.Max(lineSize, childW);
                }

                if (child.FrameworkNode is SdlGfxComponent comp)
                {
                    comp.ComponentContext.RenderToTexture(context);
                }
            }

            SDL.SDL_SetRenderTarget(context.Renderer, nint.Zero);
            return _texture;
        }
    }
}
