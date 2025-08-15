using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.LGodot.Styles;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstUIFrameworkGfxTabContainer"/>.
    /// </summary>
    public partial class LingoGodotTabContainer : TabContainer, IAbstUIFrameworkGfxTabContainer, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private Theme _theme;
        private readonly List<IAbstUIFrameworkGfxTabItem> _nodes = new List<IAbstUIFrameworkGfxTabItem>();
        private readonly ILingoGodotStyleManager _lingoGodotStyleManager;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstUIFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public string SelectedTabName => GetTabTitle(CurrentTab);
        public object FrameworkNode => this;


        public LingoGodotTabContainer(AbstUIGfxTabContainer tab, ILingoGodotStyleManager lingoGodotStyleManager)
        {
            tab.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _lingoGodotStyleManager = lingoGodotStyleManager;

            // Apply the Director tab styling
            _theme = _lingoGodotStyleManager.GetTheme(LingoGodotThemeElementType.Tabs) ?? new Theme();
            
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

        

        public void AddTab(IAbstUIFrameworkGfxTabItem tabItem)
        {
            var content = ((LingoGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Control node)
                AddTab(tabItem.Title, node);

            _nodes.Add(tabItem);
        }

        public void RemoveTab(IAbstUIFrameworkGfxTabItem tabItem)
        {
            var content = ((LingoGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Node node)
                RemoveChild(node);
            _nodes.Remove(tabItem);
        }

        public IEnumerable<IAbstUIFrameworkGfxTabItem> GetTabs() => _nodes.ToArray();
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
    public partial class LingoGodotTabItem : IAbstUIFrameworkGfxTabItem
    {
        private string _name;
        private AbstUIGfxTabItem _tabItem;
        public AbstUIGfxTabItem TabItem => _tabItem;
        public IAbstUIFrameworkGfxLayoutNode ContentFrameWork => (Content?.FrameworkObj as IAbstUIFrameworkGfxLayoutNode)!;
        public IAbstUIGfxNode? Content { get; set; }

        public float X { get => ContentFrameWork.X; set => ContentFrameWork.X = value; }
        public float Y { get => ContentFrameWork.Y; set => ContentFrameWork.Y = value; }
        public float Width { get => ContentFrameWork.Width; set => ContentFrameWork.Width = value; }
        public float Height { get => ContentFrameWork.Height; set => ContentFrameWork.Height = value; }
        public bool Visibility { get => ContentFrameWork.Visibility; set => ContentFrameWork.Visibility = value; }
        public string Title { get; set; }
        public AMargin Margin { get => ContentFrameWork.Margin; set => ContentFrameWork.Margin = value; }
        string IAbstUIFrameworkGfxNode.Name
        {
            get => ContentFrameWork.Name; set
            {
                if (ContentFrameWork != null) 
                    ContentFrameWork.Name = value;
            }
        }

        public object FrameworkNode => Content?.FrameworkObj.FrameworkNode!;

        public float TopHeight { get; set; }

        public LingoGodotTabItem(AbstUIGfxTabItem tab)
        {
            tab.Init(this);
            _tabItem = tab;
            TopHeight = LingoGodotStyle.TapItemTopHeight;
        }

        public void Dispose()
        {
        }
    }
}
