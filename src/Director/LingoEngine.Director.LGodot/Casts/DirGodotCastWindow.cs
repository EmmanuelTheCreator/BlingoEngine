﻿using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Movies;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Core;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Casts.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.LGodot.Styles;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDisposable, IHasFindMemberEvent, IDirFrameworkCastWindow,
        ICommandHandler<RemoveMemberCommand>
    {
        private readonly TabContainer _tabs;
        

        private readonly IDirectorEventMediator _mediator;
        private readonly DirectorGodotStyle _style;
        private readonly Dictionary<int, DirGodotCastView> _castViews = new();
        private readonly Dictionary<int, int> _castTabIndices = new();
        private DirGodotCastItem? _selectedItem;
        private LingoPlayer _player;
        private readonly ILingoCommandManager _commandManager;
        private readonly IHistoryManager _historyManager;
        private readonly IDirectorIconManager _iconManager;

        public ILingoCast? ActiveCastLib { get; private set; }
        internal ILingoMember? SelectedMember => _selectedItem?.LingoMember;

        public DirGodotCastWindow(IDirectorEventMediator mediator, DirectorGodotStyle style, DirectorCastWindow directorCastWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, ILingoCommandManager commandManager, IHistoryManager historyManager, IDirectorIconManager iconManager)
            : base(DirectorMenuCodes.CastWindow, "Cast", windowManager, historyManager)
        {
            _mediator = mediator;
            _style = style;
            directorCastWindow.Init(this);
            _player = (LingoPlayer)player;
            _commandManager = commandManager;
            _historyManager = historyManager;
            _iconManager = iconManager;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator.Subscribe(this);

            Size = new Vector2(360, 620);
            CustomMinimumSize = Size;
            
            _tabs = new TabContainer();
            _tabs.Position = new Vector2(0, TitleBarHeight );

            InitTabs();
            AddChild(_tabs);
        }


        protected override void OnResizing(Vector2 size)
        {
            base.OnResizing(size);
            _tabs.Size = new Vector2(Size.X , Size.Y- TitleBarHeight -10);
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            SetActiveMovie(movie as LingoMovie);
        }

        public void SetActiveMovie(ILingoMovie? lingoMovie)
        {
            for (int i = _tabs.GetChildCount() - 1; i >= 0; i--)
            {
                var child = _tabs.GetChild(i);
                _tabs.RemoveChild(child);
                child.QueueFree();
            }
            _castViews.Clear();
            _castTabIndices.Clear();

            if (lingoMovie == null) return;

            int index = 0;
            foreach (var cast in lingoMovie.CastLib.GetAll())
            {
                var castLibViewer = new DirGodotCastView(OnSelectElement, _style, _commandManager, _iconManager, _player.Factory);
                castLibViewer.Show(cast);
                _castViews.Add(cast.Number, castLibViewer);
                _castTabIndices.Add(cast.Number, index);
                var tabContent = new VBoxContainer
                {
                    Name = cast.Name,
                };
                tabContent.MouseFilter = Control.MouseFilterEnum.Stop;
                tabContent.AddChild(castLibViewer.Node);
                _tabs.AddChild(tabContent);
                index++;
            }
        }


        private void InitTabs()
        {
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.Size = new Vector2(Size.X, Size.Y - TitleBarHeight - 10);
        }

        private void OnSelectElement(DirGodotCastItem castItem)
        {
            _selectedItem?.SetSelected(false);
            castItem.SetSelected(true);
            _selectedItem = castItem;
            _mediator.RaiseMemberSelected(castItem.LingoMember);

        }

        public void FindMember(ILingoMember member)
        {
            if (_castViews.TryGetValue(member.CastLibNum, out var view))
            {
                if (_castTabIndices.TryGetValue(member.CastLibNum, out var index))
                    _tabs.CurrentTab = index;
                var item = view.FindItem(member);
                if (item != null)
                    view.SelectItem(item);
            }
        }
        public void Activate(int castlibNum)
        {
            //ActiveCastLib = _lingoMovie.CastLib.GetCast(castlibNum);
            //if (ActiveCastLib != null)
            //    CastLibViewer.Show(ActiveCastLib);
        }

        public new void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _mediator.Unsubscribe(this);
            base.Dispose();
        }

        public bool CanExecute(RemoveMemberCommand command) => true;
        public bool Handle(RemoveMemberCommand command)
        {
            var cast = command.Cast;
            var member = command.Member;

            int number = member.NumberInCast;
            string name = member.Name;
            string fileName = member.FileName;
            var type = member.Type;
            var reg = member.RegPoint;

            cast.Remove(member);

            ILingoMember current = member;

            void refresh() => SetActiveMovie(_player.ActiveMovie as LingoMovie);

            Action undo = () =>
            {
                current = cast.Add(type, number, name, fileName, reg);
                refresh();
            };

            Action redo = () =>
            {
                cast.Remove((LingoMember)current);
                refresh();
            };

            _historyManager.Push(undo, redo);
            refresh();
            return true;
        }

       
    }
}
