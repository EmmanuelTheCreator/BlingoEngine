using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkWrapPanel"/>.
    /// </summary>
    public partial class AbstGodotWrapPanel : MarginContainer, IAbstFrameworkWrapPanel, IDisposable, IFrameworkFor<AbstWrapPanel>
    {
        private FlowContainer _container;
        private AOrientation _orientation;
        private APoint _itemMargin;
        private AMargin _margin;


        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => CustomMinimumSize.X;
            set
            {
                if (_orientation == AOrientation.Horizontal)
                    CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y);
                else
                    CustomMinimumSize = new Vector2(value, 0); // auto if vertical
            }
        }

        public float Height
        {
            get => CustomMinimumSize.Y;
            set
            {
                if (_orientation == AOrientation.Vertical)
                    CustomMinimumSize = new Vector2(CustomMinimumSize.X, value);
                else
                    CustomMinimumSize = new Vector2(0, value); // auto if horizontal
            }
        }

        public bool Visibility { get => Visible; set => Visible = value; }

        //public SizeFlags SizeFlagsHorizontal { get => _container.SizeFlagsHorizontal; set => _container.SizeFlagsHorizontal = value; }
        //public Vector2 Position { get => _container.Position; set => _container.Position = value; }
        string IAbstFrameworkNode.Name
        {
            get => Name; set
            {
                Name = value; _container.Name = value + "_Flow";
            }
        }

        public AOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value)
                    return;
                var children = _container.GetChildren().OfType<Node>().ToArray();
                foreach (var c in children)
                    _container.RemoveChild(c);
                RemoveChild(_container);
                _container.QueueFree();
                _orientation = value;
                _container = CreateContainer(value);
                AddChild(_container);
                ApplyMargin();
                foreach (var c in children)
                {
                    _container.AddChild(c);
                    if (c is Control ctrl)
                        ApplyItemMargin(ctrl);
                }
            }
        }

        public APoint ItemMargin
        {
            get => _itemMargin;
            set
            {
                _itemMargin = value;
                //foreach (var child in _container.GetChildren().OfType<Control>())
                //    ApplyItemMargin(child);
                _container.AddThemeConstantOverride("h_separation", (int)_itemMargin.X);
                _container.AddThemeConstantOverride("v_separation", (int)_itemMargin.Y);
            }
        }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                ApplyMargin();
            }
        }


        public AbstGodotWrapPanel(AbstWrapPanel panel, AOrientation orientation)
        {
            _orientation = orientation;
            _itemMargin = new APoint(4, 4);
            _margin = AMargin.Zero;
            MouseFilter = MouseFilterEnum.Ignore;
            _container = CreateContainer(orientation);
            AddChild(_container);
            panel.Init(this);
        }

        private FlowContainer CreateContainer(AOrientation orientation)
        {
            FlowContainer container;

            if (orientation == AOrientation.Vertical)
            {
                container = new VFlowContainer();
                container.SizeFlagsVertical = SizeFlags.ExpandFill; // main axis (needs parent height)
                container.SizeFlagsHorizontal = 0;                  // auto width
            }
            else
            {
                container = new HFlowContainer();
                container.SizeFlagsHorizontal = SizeFlags.ExpandFill; // main axis (needs parent width)
                container.SizeFlagsVertical = 0;                      // auto height
            }

            container.AddThemeConstantOverride("h_separation", 4);
            container.AddThemeConstantOverride("v_separation", 4);
            return container;
        }

        public override void _Ready()
        {
            base._Ready();

            // Let parent define available rect (important for wrapping)
            this.SetAnchorsPreset(LayoutPreset.FullRect);

            // Mirror orientation onto THIS MarginContainer (panel)
            if (_orientation == AOrientation.Vertical)
            {
                SizeFlagsVertical = SizeFlags.ExpandFill;  // main axis
                SizeFlagsHorizontal = 0;                   // cross axis = auto
            }
            else
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill; // main axis
                SizeFlagsVertical = 0;                      // cross axis = auto
            }


            ApplyMargin();
        }

       
        public object FrameworkNode => this;

        private readonly List<IAbstFrameworkNode> _nodes = new List<IAbstFrameworkNode>();
        public void AddItem(IAbstFrameworkNode child)
        {
            if (child.FrameworkNode is not Node node)
                return;

            if (node is Control ctrl)
                ApplyItemMargin(ctrl);
            _container.AddChild(node);
            _nodes.Add(child);
        }
        public void RemoveItem(IAbstFrameworkNode child)
        {
            if (child.FrameworkNode is not Node node)
                return;
            _container.RemoveChild(node);
            _nodes.Remove(child);
        }
        public IEnumerable<IAbstFrameworkNode> GetItems() => _nodes.ToArray();
        public IAbstFrameworkNode? GetItem(int index) => _nodes[index];
        public void RemoveAll()
        {
            foreach (var child in GetItems())
            {
                if (child != GetItem(0))
                    RemoveItem(child);
            }
            _nodes.Clear();
        }


        public new void Dispose()
        {
            RemoveAll();
            QueueFree();
            base.Dispose();
        }

        private void ApplyItemMargin(Control ctrl)
        {
            //ctrl.AddThemeConstantOverride("margin_left", (int)_itemMargin.Left);
            //ctrl.AddThemeConstantOverride("margin_right", (int)_itemMargin.Right);
            //ctrl.AddThemeConstantOverride("margin_top", (int)_itemMargin.Top);
            //ctrl.AddThemeConstantOverride("margin_bottom", (int)_itemMargin.Bottom);
        }

        private void ApplyMargin()
        {
            AddThemeConstantOverride("margin_left", (int)_margin.Left);
            AddThemeConstantOverride("margin_right", (int)_margin.Right);
            AddThemeConstantOverride("margin_top", (int)_margin.Top);
            AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }


    }
}
