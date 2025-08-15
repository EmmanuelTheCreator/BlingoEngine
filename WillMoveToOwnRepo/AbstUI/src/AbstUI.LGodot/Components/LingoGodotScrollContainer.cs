using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using System.Linq;

namespace LingoEngine.LGodot.Gfx
{
    public partial class LingoGodotScrollContainer : ScrollContainer, IAbstUIFrameworkGfxScrollContainer, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        public LingoGodotScrollContainer(AbstUIGfxScrollContainer container)
        {
            container.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstUIFrameworkGfxNode.Name { get => Name; set => Name = value; }

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
        public object FrameworkNode => this;


        private readonly List<IAbstUIFrameworkGfxLayoutNode> _nodes = new List<IAbstUIFrameworkGfxLayoutNode>();
        public void AddItem(IAbstUIFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                base.AddChild(node);
            _nodes.Add(child);
        }
        public void RemoveItem(IAbstUIFrameworkGfxLayoutNode lingoFrameworkGfxNode)
        {
            if (lingoFrameworkGfxNode.FrameworkNode is Node node)
                RemoveChild(node);
            _nodes.Remove(lingoFrameworkGfxNode);
        }

        public IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();



        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

       
    }
}
