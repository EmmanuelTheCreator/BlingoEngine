using System.Collections.Generic;
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

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow>
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoGfxTabContainer _tabs;
        private readonly Dictionary<string, DirCastTab> _tabMap = new();
        private readonly ILingoCommandManager _commandManager;
        private readonly IDirectorIconManager _iconManager;
        private ILingoMember? _selected;
        private ILingoMouseSubscription? _mouseSub;

        public LingoGfxTabContainer TabContainer => _tabs;
        public ILingoMember? SelectedMember => _selected;

        public DirectorCastWindow(ILingoFrameworkFactory factory, IDirectorEventMediator mediator, ILingoCommandManager commandManager, IDirectorIconManager iconManager) : base(factory)
        {
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
            base.Dispose();
        }

        public void LoadMovie(ILingoMovie? movie)
        {
            Framework.SetActiveMovie(movie);
            _tabMap.Clear();
            _tabs.ClearTabs();
            if (movie == null)
                return;

            foreach (var cast in movie.CastLib.GetAll())
            {
                var members = cast.GetAll();
                var tabName = cast.Name ?? $"Cast{cast.Number}";
                var tab = new DirCastTab(_factory, tabName, members, _iconManager, _commandManager);
                var tabItem = _factory.CreateTabItem(tabName, tabName);
                tabItem.Content = tab.Scroll;
                _tabs.AddTab(tabItem);
                _tabMap[tabItem.Title] = tab;
                tab.MemberSelected += (m, i) => OnMemberSelected(tab, m, i);
            }
        }

        public void SetViewportWidth(int width)
        {
            foreach (var tab in _tabMap.Values)
                tab.SetViewportWidth(width);
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

