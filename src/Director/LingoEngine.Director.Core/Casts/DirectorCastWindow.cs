using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.Events;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Core;
using LingoEngine.Director.Core.Events;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow> , IHasFindMemberEvent
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoGfxTabContainer _tabs;
        private readonly LingoPlayer _player;
        private readonly Dictionary<string, DirCastTab> _tabMap = new();
        private readonly ILingoCommandManager _commandManager;
        private readonly IDirectorIconManager _iconManager;
        private ILingoMember? _selected;
        private ILingoMouseSubscription? _mouseSub;

        public LingoGfxTabContainer TabContainer => _tabs;
        public ILingoMember? SelectedMember => _selected;
        public int Width { get; set; } = 370;
        public int Height { get; set; } = 620;

        public DirectorCastWindow(ILingoFrameworkFactory factory, IDirectorEventMediator mediator, ILingoCommandManager commandManager, IDirectorIconManager iconManager, ILingoPlayer player) : base(factory)
        {
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator = mediator;
            _factory = factory;
            _commandManager = commandManager;
            _iconManager = iconManager;
            _tabs = factory.CreateTabContainer("CastTabs");
        }

        public override void Init(IDirFrameworkWindow frameworkWindow)
        {
            base.Init(frameworkWindow);
            _mouseSub = Mouse.OnMouseEvent(OnMouseEvent);
        }

        public override void Dispose()
        {
            _mouseSub?.Release();
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            base.Dispose();
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            SetActiveMovie(movie);
        }

        public void SetActiveMovie(ILingoMovie? lingoMovie)
        {
            LoadMovie(lingoMovie);
            SetViewportSize(Width, Height);
        }

        public void LoadMovie(ILingoMovie? movie)
        {
            _tabMap.Clear();
            foreach (var tab in _tabMap)
                tab.Value.Dispose();
            _tabs.ClearTabs();
            if (movie == null)
                return;

            foreach (var cast in movie.CastLib.GetAll())
            {
                var tab = new DirCastTab(_factory, cast, _iconManager, _commandManager);
                _tabs.AddTab(tab.TabItem);
                _tabMap[tab.TabItem.Title] = tab;
                tab.MemberSelected += (m, i) => OnMemberSelected(tab, m, i);

                tab.LoadAllMembers();
            }
        }
        public void OnResizing(int width, int height)
        {
            Width = width;
            SetViewportSize(width, height);
        }
        private void SetViewportSize(int width, int height)
        {
            foreach (var tab in _tabMap.Values)
                tab.SetViewportSize(width, height);
        }

        private void OnMouseEvent(LingoMouseEvent e)
        {
            if (_tabMap.TryGetValue(_tabs.SelectedTabName, out var tab))
                tab.HandleMouseEvent(e);
        }

        private void OnMemberSelected(DirCastTab source, ILingoMember member, DirCastItem item)
        {
            _selected = member;
            foreach (var kv in _tabMap)
                if (kv.Value != source)
                    kv.Value.Select(null);
            _mediator.RaiseMemberSelected(member);
        }

        public void FindMember(ILingoMember member)
        {
            foreach (var kv in _tabMap)
            {
                int idx = kv.Value.IndexOfMember(member);
                if (idx >= 0)
                {
                    _tabs.SelectTabByName(kv.Key);
                    kv.Value.ScrollToIndex(idx);
                    _selected = member;
                    kv.Value.Select(kv.Value.GetItem(idx));
                    break;
                }
            }
        }
    }
}

