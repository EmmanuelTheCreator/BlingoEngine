using System;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using Godot;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="AbstZoomBox"/> hosting a single child.
    /// </summary>
    public partial class AbstGodotZoomBox : Control, IAbstFrameworkZoomBox, IFrameworkFor<AbstZoomBox>, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private IAbstFrameworkLayoutNode? _content;

        public AbstGodotZoomBox(AbstZoomBox zoomBox)
        {
            MouseFilter = MouseFilterEnum.Ignore;
            zoomBox.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                ApplyMargin();
            }
        }

        public object FrameworkNode => this;

        public IAbstFrameworkLayoutNode? Content
        {
            get => _content;
            set
            {
                if (_content != null && _content.FrameworkNode is Node oldNode)
                    RemoveChild(oldNode);
                _content = value;
                if (_content != null && _content.FrameworkNode is Node node)
                {
                    AddChild(node);
                    if (node is Control control)
                    {
                        control.AnchorLeft = 0;
                        control.AnchorTop = 0;
                        control.AnchorRight = 0;
                        control.AnchorBottom = 0;
                        control.Position = new Vector2(_content.X, _content.Y);
                    }
                }
            }
        }

        float IAbstFrameworkZoomBox.ScaleH
        {
            get => Scale.X;
            set => Scale = new Vector2(value, Scale.Y);
        }

        float IAbstFrameworkZoomBox.ScaleV
        {
            get => Scale.Y;
            set => Scale = new Vector2(Scale.X, value);
        }

        private void ApplyMargin()
        {
            AddThemeConstantOverride("margin_left", (int)_margin.Left);
            AddThemeConstantOverride("margin_right", (int)_margin.Right);
            AddThemeConstantOverride("margin_top", (int)_margin.Top);
            AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }

        public new void Dispose()
        {
            if (_content != null && _content.FrameworkNode is Node node)
                RemoveChild(node);
            base.Dispose();
            QueueFree();
        }
    }
}
