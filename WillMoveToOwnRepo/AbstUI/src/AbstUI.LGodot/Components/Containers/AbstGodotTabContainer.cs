using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkTabContainer"/>.
    /// </summary>
    public partial class AbstGodotTabContainer : TabContainer, IAbstFrameworkTabContainer, IDisposable, IFrameworkFor<AbstTabContainer>
    {
        private AMargin _margin = AMargin.Zero;
        private Theme _theme;
        private readonly List<IAbstFrameworkTabItem> _nodes = new List<IAbstFrameworkTabItem>();
        private readonly IAbstGodotStyleManager _lingoGodotStyleManager;



        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => Size.X;
            set
            {
                CustomMinimumSize = new Vector2(value, Size.Y);
                Size = CustomMinimumSize;
            }
        }
        public float Height
        {
            get => Size.Y;
            set
            {
                CustomMinimumSize = new Vector2(Size.X, value);
                Size = CustomMinimumSize;
            }
        }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
        public string SelectedTabName => GetTabTitle(CurrentTab);
        public object FrameworkNode => this;


        public AbstGodotTabContainer(AbstTabContainer tab, IAbstGodotStyleManager lingoGodotStyleManager)
        {
            tab.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _lingoGodotStyleManager = lingoGodotStyleManager;

            // Apply the Director tab styling
            _theme = _lingoGodotStyleManager.GetTheme(AbstGodotThemeElementType.Tabs) ?? new Theme();

            Theme = Theme;
            ClipTabs = true;

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



        public void AddTab(IAbstFrameworkTabItem tabItem)
        {

            var content = ((AbstGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Control node)
                AddTab(tabItem.Title, node);

            _nodes.Add(tabItem);
        }

        public void RemoveTab(IAbstFrameworkTabItem tabItem)
        {
            var content = ((AbstGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Node node)
                RemoveChild(node);
            _nodes.Remove(tabItem);
        }

        public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _nodes.ToArray();
        public void AddTab(string title, Node node)
        {
            AddChild(node);
            SetTabTitle(GetChildCount() - 1, title);
        }

        public void ClearTabs()
        {
            foreach (Node child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }
            _nodes.Clear();
        }

        public new void Dispose()
        {
            ClearTabs();
            QueueFree();
            base.Dispose();
        }

        public void SelectTabByName(string tabName)
        {
            CurrentTab = _nodes.FindIndex(x => x.Title == tabName);
        }
    }
    public partial class AbstGodotTabItem : IAbstFrameworkTabItem, IFrameworkFor<AbstTabItem>
    {
        private AbstTabItem _tabItem;
        public AbstTabItem TabItem => _tabItem;
        public IAbstFrameworkLayoutNode ContentFrameWork => (Content?.FrameworkObj as IAbstFrameworkLayoutNode)!;
        public IAbstNode? Content { get; set; }

        // To make a better way for styling
        public static float TabItemTopHeight { get; set; } = 22;

        public float X { get => ContentFrameWork.X; set => ContentFrameWork.X = value; }
        public float Y { get => ContentFrameWork.Y; set => ContentFrameWork.Y = value; }
        public float Width { get => ContentFrameWork.Width; set => ContentFrameWork.Width = value; }
        public float Height { get => ContentFrameWork.Height; set => ContentFrameWork.Height = value; }
        public bool Visibility { get => ContentFrameWork.Visibility; set => ContentFrameWork.Visibility = value; }
        public string Title { get; set; } = "";
        public AMargin Margin { get => ContentFrameWork.Margin; set => ContentFrameWork.Margin = value; }
        string IAbstFrameworkNode.Name
        {
            get => ContentFrameWork.Name; set
            {
                if (ContentFrameWork != null)
                    ContentFrameWork.Name = value;
            }
        }

        public object FrameworkNode => Content?.FrameworkObj.FrameworkNode!;

        public float TopHeight { get; set; }

        public AbstGodotTabItem(AbstTabItem tab)
        {
            tab.Init(this);
            _tabItem = tab;
            TopHeight = TabItemTopHeight;
        }

        public void Dispose()
        {
        }
    }
}
