using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Components.Containers
{
    /// <summary>
    /// SDL implementation of <see cref="AbstZoomBox"/> using a single child panel.
    /// </summary>
    public class AbstSdlZoomBox : AbstSdlPanel, IAbstFrameworkZoomBox, IFrameworkFor<AbstZoomBox>
    {
        private IAbstFrameworkLayoutNode? _content;
        private float _scaleH = 1f;
        private float _scaleV = 1f;

        public AbstSdlZoomBox(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public IAbstFrameworkLayoutNode? Content
        {
            get => _content;
            set
            {
                if (_content != null)
                    base.RemoveItem(_content);
                _content = value;
                if (_content != null)
                    base.AddItem(_content);
            }
        }

        public new void AddItem(IAbstFrameworkLayoutNode child)
            => throw new NotSupportedException("Use Content property");

        public new void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_content == child)
                Content = null;
        }

        public new void RemoveAll() => Content = null;

        public new IEnumerable<IAbstFrameworkLayoutNode> GetItems()
        {
            if (_content != null)
                yield return _content;
        }

        public float ScaleH
        {
            get => _scaleH;
            set => _scaleH = value;
        }

        public float ScaleV
        {
            get => _scaleV;
            set => _scaleV = value;
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            SDL.SDL_RenderGetScale(context.Renderer, out var oldSX, out var oldSY);
            SDL.SDL_RenderSetScale(context.Renderer, _scaleH, _scaleV);
            var result = base.Render(context);
            SDL.SDL_RenderSetScale(context.Renderer, oldSX, oldSY);
            return result;
        }

        public override void HandleEvent(AbstSDLEvent e)
        {
            if (_content?.FrameworkNode is AbstSdlComponent comp && comp is IHandleSdlEvent handler)
            {
                var oriX = e.OffsetX;
                var oriY = e.OffsetY;
                e.OffsetX = (oriX - _content.X) / _scaleH;
                e.OffsetY = (oriY - _content.Y) / _scaleV;
                e.CalulateIsInside(comp.Width, comp.Height);
                if (handler.CanHandleEvent(e))
                    handler.HandleEvent(e);
                e.OffsetX = oriX;
                e.OffsetY = oriY;
            }
        }
    }
}
