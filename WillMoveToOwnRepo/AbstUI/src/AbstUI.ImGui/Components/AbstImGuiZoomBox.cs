using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiZoomBox : AbstImGuiPanel, IAbstFrameworkZoomBox, IFrameworkFor<AbstZoomBox>, IDisposable
    {
        private IAbstFrameworkLayoutNode? _content;
        private float _scaleH = 1f;
        private float _scaleV = 1f;

        public AbstImGuiZoomBox(AbstImGuiComponentFactory factory) : base(factory)
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
    }
}
