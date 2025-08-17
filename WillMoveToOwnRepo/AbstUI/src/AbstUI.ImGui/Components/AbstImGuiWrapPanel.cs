using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiWrapPanel : AbstImGuiComponent, IAbstFrameworkWrapPanel, IDisposable
    {
        public AOrientation Orientation { get; set; }
        public APoint ItemMargin { get; set; }
        public AMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public AbstImGuiWrapPanel(AbstImGuiComponentFactory factory, AOrientation orientation) : base(factory)
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
            base.Dispose();
        }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

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

                if (child.FrameworkNode is AbstImGuiComponent comp)
                {
                    comp.ComponentContext.RenderToTexture(context);
                }
            }

            // TODO: draw wrap panel using ImGui
            return AbstImGuiRenderResult.RequireRender();
        }
    }
}
