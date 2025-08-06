using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Tools;
﻿using Godot;
using System;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.UI;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IHasFindMemberEvent, IDirFrameworkCastWindow
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly LingoPlayer _player;
        private readonly DirectorCastWindow _directorCastWindow;
        private readonly LingoGodotTabContainer _tabs;
        internal ILingoMember? SelectedMember => _directorCastWindow.SelectedMember;

        public DirGodotCastWindow(IDirectorEventMediator mediator, DirectorCastWindow directorCastWindow, ILingoPlayer player, IDirGodotWindowManager windowManager)
            : base(DirectorMenuCodes.CastWindow, "Cast", windowManager)
        {
            _mediator = mediator;
            _directorCastWindow = directorCastWindow;
            directorCastWindow.Init(this);
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator.Subscribe(this);

            Size = new Vector2(360, 620);
            CustomMinimumSize = Size;


            _tabs = new TabContainer();
            _tabs.Position = new Vector2(0, TitleBarHeight );


            _tabs = _directorCastWindow.TabContainer.Framework<LingoGodotTabContainer>();
            _tabs.Position = new Vector2(0, TitleBarHeight);
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            AddChild(_tabs);
        }

        protected override void OnResizing(Vector2 size)
        {
            base.OnResizing(size);
            _tabs.Size = new Vector2(Size.X, Size.Y - TitleBarHeight - 10);
            _directorCastWindow.SetViewportWidth((int)_tabs.Size.X);
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            SetActiveMovie(movie);
        }

        public void SetActiveMovie(ILingoMovie? lingoMovie)
        {
            _directorCastWindow.LoadMovie(lingoMovie);
            _directorCastWindow.SetViewportWidth((int)_tabs.Size.X);
        }

        public void FindMember(ILingoMember member)
        {
            _directorCastWindow.FindMember(member);
        }

        public new void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _mediator.Unsubscribe(this);
            base.Dispose();
        }
    }
}
