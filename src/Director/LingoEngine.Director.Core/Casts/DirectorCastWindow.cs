using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Inputs;
using AbstUI.Windowing;
using LingoEngine.Core;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow> , IHasFindMemberEvent
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly AbstTabContainer _tabs;
        private readonly LingoPlayer _player;
        private readonly Dictionary<string, DirCastTab> _tabMap = new();
        private readonly IAbstCommandManager _commandManager;
        private readonly IDirectorIconManager _iconManager;
        private ILingoMember? _selected;
        private IAbstMouseSubscription? _mouseSub;
        private ILingoMovie? _movie;

        public AbstTabContainer TabContainer => _tabs;
        public ILingoMember? SelectedMember => _selected;

        public DirectorCastWindow(IServiceProvider serviceProvider, ILingoFrameworkFactory factory, IDirectorEventMediator mediator, IAbstCommandManager commandManager, IDirectorIconManager iconManager, ILingoPlayer player) : base(serviceProvider, DirectorMenuCodes.CastWindow)
        {
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator = mediator;
            _commandManager = commandManager;
            _iconManager = iconManager;
            MinimumWidth = 360;
            MinimumHeight = 100;
            Width = 370;
            Height = 620;
            X = 830;
            Y = 22;
            _tabs = factory.CreateTabContainer("CastTabs");
            //_tabs.Height = Height;
            _mediator.Subscribe(this);
            
        }
       
        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            _mouseSub = MouseT.OnMouseEvent(OnMouseEvent);
            Content = _tabs;
        }
       
        protected override void OnDispose()
        {
            _mediator.Unsubscribe(this);
            _mouseSub?.Release();
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            base.OnDispose();
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (movie == _movie)
                return;
            _movie = movie;
            SetActiveMovie(movie);
        }

        private void SetActiveMovie(ILingoMovie? movie)
        {
            _tabMap.Clear();
            foreach (var tab in _tabMap)
                tab.Value.Dispose();
            _tabs.ClearTabs();
            if (movie == null)
                return;

            foreach (var cast in movie.CastLib.GetAll())
            {
                var tab = new DirCastTab(_factory, cast, _iconManager, _commandManager, _mediator, _player);
                tab.SetViewportSize((int)_tabs.Width,(int) _tabs.Height);
                _tabs.AddTab(tab.TabItem);
                _tabMap[tab.TabItem.Title] = tab;
                tab.MemberSelected += (m, i) => OnMemberSelected(tab, m, i);

                tab.LoadAllMembers();
            }
        }
        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            _tabs.Width = width;
            _tabs.Height = height-10;
            foreach (var tab in _tabMap.Values)
                tab.SetViewportSize(width, height-10);
        }

        private void OnMouseEvent(AbstMouseEvent e)
        {
            if (_tabMap.TryGetValue(_tabs.SelectedTabName, out var tab))
                tab.HandleMouseEvent(e);
        }

        private void OnMemberSelected(DirCastTab source, ILingoMember member, IDirCastItem item)
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

