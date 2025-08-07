using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxWrapPanel : ILingoFrameworkGfxWrapPanel, IDisposable, ISdlRenderElement
    {
        private readonly nint _renderer;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoOrientation Orientation { get; set; }
        public LingoPoint ItemMargin { get; set; }
        public LingoMargin Margin { get; set; }
        public object FrameworkNode => this;

        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();

        public SdlGfxWrapPanel(nint renderer, LingoOrientation orientation)
        {
            _renderer = renderer;
            Orientation = orientation;
            ItemMargin = new LingoPoint(0, 0);
            Margin = LingoMargin.Zero;
        }

        public void AddItem(ILingoFrameworkGfxNode child)
        {
            if (child is ILingoFrameworkGfxLayoutNode layout && !_children.Contains(layout))
                _children.Add(layout);
        }

        public void RemoveItem(ILingoFrameworkGfxNode child)
        {
            if (child is ILingoFrameworkGfxLayoutNode layout)
                _children.Remove(layout);
        }

        public IEnumerable<ILingoFrameworkGfxNode> GetItems() => _children.ToArray();

        public ILingoFrameworkGfxNode? GetItem(int index)
        {
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index];
        }

        public void RemoveAll()
        {
            _children.Clear();
        }

        public void Dispose()
        {
            RemoveAll();
        }

        public void Render()
        {
            if (!Visibility) return;

            ImGui.SetCursorPos(new Vector2(X, Y));
            ImGui.PushID(Name);
            ImGui.BeginChild("##wrap", new Vector2(Width, Height), false);
            float curX = 0;
            float curY = 0;
            float lineSize = 0;
            foreach (var child in _children)
            {
                var margin = child.Margin;
                float childW = child.Width + margin.Left + margin.Right;
                float childH = child.Height + margin.Top + margin.Bottom;

                if (Orientation == LingoOrientation.Horizontal)
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

                if (child.FrameworkNode is ISdlRenderElement renderable)
                    renderable.Render();
            }

            ImGui.EndChild();
            ImGui.PopID();
        }
    }
}
