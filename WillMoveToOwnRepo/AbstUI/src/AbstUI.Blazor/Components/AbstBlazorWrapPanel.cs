using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorWrapPanel : AbstBlazorComponent, IAbstFrameworkWrapPanel, IDisposable
    {
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public AbstBlazorWrapPanel(AbstBlazorComponentFactory factory, AOrientation orientation) : base(factory)
        {
            Orientation = orientation;
            ItemMargin = new APoint(0, 0);
            Margin = AMargin.Zero;
        }

        public void AddItem(IAbstFrameworkNode child)
        {
            if (child is IAbstFrameworkLayoutNode layout && !_children.Contains(layout))
                _children.Add(layout);
        }

        public void RemoveItem(IAbstFrameworkNode child)
        {
            if (child is IAbstFrameworkLayoutNode layout)
                _children.Remove(layout);
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
                Blazor.Blazor_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }

        private nint _texture;
        private int _texW;
        private int _texH;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
