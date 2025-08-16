using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components
{
    internal class AbstBlazorPanel : AbstBlazorComponent, IAbstFrameworkPanel, IDisposable
    {
        public AbstBlazorPanel(AbstBlazorComponentFactory factory) : base(factory)
        {
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public AColor? BackgroundColor { get; set; }
        public AColor? BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Remove(child))
                (child as IDisposable)?.Dispose();
        }

        public void RemoveAll()
        {
            foreach (var child in _children)
                (child as IDisposable)?.Dispose();
            _children.Clear();
        }

        private nint _texture;
        private int _texW;
        private int _texH;

        public override AbstBlazorRenderResult Render(AbstBlazorRenderContext context) => new AbstBlazorRenderResult();
    }
}
