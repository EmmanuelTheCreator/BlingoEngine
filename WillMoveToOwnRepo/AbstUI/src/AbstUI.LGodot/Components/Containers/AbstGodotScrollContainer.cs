using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using System.Linq;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    public partial class AbstGodotScrollContainer : ScrollContainer, IAbstFrameworkScrollContainer, IDisposable, IFrameworkFor<AbstScrollContainer>
    {
        private AMargin _margin = AMargin.Zero;
        private AbstScrollbarMode _scrollModeH = AbstScrollbarMode.Auto;
        private AbstScrollbarMode _scrollModeV = AbstScrollbarMode.Auto;
        private readonly List<IAbstFrameworkLayoutNode> _nodes = new List<IAbstFrameworkLayoutNode>();

        public object FrameworkNode => this;
        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _nodes.ToArray();
        public new float ScrollHorizontal
        {
            get => base.ScrollHorizontal;
            set => base.ScrollHorizontal = (int)value;
        }
        public new float ScrollVertical
        {
            get => base.ScrollVertical;
            set => base.ScrollVertical = (int)value;
        }
        public new bool ClipContents
        {
            get => base.ClipContents;
            set => base.ClipContents = value;
        }

        public AbstScrollbarMode ScrollbarModeH
        {
            get => _scrollModeH;
            set
            {
                _scrollModeH = value;
                HorizontalScrollMode = value switch
                {
                    AbstScrollbarMode.Hidden => ScrollMode.ShowNever,
                    AbstScrollbarMode.AlwaysVisible => ScrollMode.ShowAlways,
                    _ => ScrollMode.Auto,
                };
            }
        }

        public AbstScrollbarMode ScrollbarModeV
        {
            get => _scrollModeV;
            set
            {
                _scrollModeV = value;
                VerticalScrollMode = value switch
                {
                    AbstScrollbarMode.Hidden => ScrollMode.ShowNever,
                    AbstScrollbarMode.AlwaysVisible => ScrollMode.ShowAlways,
                    _ => ScrollMode.Auto,
                };
            }
        }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }






        public AbstGodotScrollContainer(AbstScrollContainer container)
        {
            container.Init(this);
            ScrollbarModeH = _scrollModeH;
            ScrollbarModeV = _scrollModeV;
        }


        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                base.AddChild(node);
            _nodes.Add(child);
        }
        public void RemoveItem(IAbstFrameworkLayoutNode lingoFrameworkGfxNode)
        {
            if (lingoFrameworkGfxNode.FrameworkNode is Node node)
                RemoveChild(node);
            _nodes.Remove(lingoFrameworkGfxNode);
        }





        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }


    }
}
