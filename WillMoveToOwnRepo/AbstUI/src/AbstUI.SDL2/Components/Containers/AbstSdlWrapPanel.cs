using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components.Containers
{
    internal class AbstSdlWrapPanel : AbstSdlComponent, IAbstFrameworkWrapPanel, IDisposable
    {
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkNode> _children = new();

        public AbstSdlWrapPanel(AbstSdlComponentFactory factory, AOrientation orientation) : base(factory)
        {
            Orientation = orientation;
            ItemMargin = new APoint(0, 0);
            Margin = AMargin.Zero;
        }

        public void AddItem(IAbstFrameworkNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public void RemoveItem(IAbstFrameworkNode child)
        {
            _children.Remove(child);
        }

        public IEnumerable<IAbstFrameworkNode> GetItems() => _children.ToArray();

        public IAbstFrameworkNode? GetItem(int index)
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

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
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

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
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

                float targetX;
                float targetY;

                if (Orientation == AOrientation.Horizontal)
                {
                    if (curX + childW > Width)
                    {
                        curX = 0;
                        curY += lineSize + ItemMargin.Y;
                        lineSize = 0;
                    }
                    targetX = curX + margin.Left;
                    targetY = curY + margin.Top;
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
                    targetX = curX + margin.Left;
                    targetY = curY + margin.Top;
                    curY += childH + ItemMargin.Y;
                    lineSize = Math.Max(lineSize, childW);
                }

                var comp = child.FrameworkNode as AbstSdlComponent;

                if (child is IAbstFrameworkLayoutNode layout)
                {
                    layout.X = targetX;
                    layout.Y = targetY;
                }
                else if (comp != null)
                {
                    comp.X = targetX;
                    comp.Y = targetY;
                }

                comp?.ComponentContext.RenderToTexture(context);
            }

            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            return _texture;
        }
    }
}
