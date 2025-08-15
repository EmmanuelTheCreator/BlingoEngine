using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Tools;
﻿using Godot;
using LingoEngine.Director.Core.UI;
using LingoEngine.Members;
using LingoEngine.Director.LGodot.Windowing;
using AbstUI.LGodot.Components;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDirFrameworkCastWindow
    {
        private readonly DirectorCastWindow _directorCastWindow;
        private readonly AbstGodotTabContainer _tabs;
        internal ILingoMember? SelectedMember => _directorCastWindow.SelectedMember;

        public DirGodotCastWindow(DirectorCastWindow directorCastWindow, IDirGodotWindowManager windowManager, IHistoryManager historyManager)
            : base(DirectorMenuCodes.CastWindow, "Cast", windowManager, historyManager)
        {
            _directorCastWindow = directorCastWindow;
            directorCastWindow.Init(this);
            
            //_mediator.Subscribe(this);

            Size = new Vector2(directorCastWindow.Width, directorCastWindow.Height);
            CustomMinimumSize = Size;

            _tabs = _directorCastWindow.TabContainer.Framework<AbstGodotTabContainer>();
            _tabs.Position = new Vector2(0, TitleBarHeight);
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            AddChild(_tabs);
            UpdateSize(Size);
        }

        protected override void OnResizing(Vector2 size)
        {
            base.OnResizing(size);
            UpdateSize(size);
        }

        private void UpdateSize(Vector2 size)
        {
            _tabs.Size = new Vector2(Size.X, Size.Y - TitleBarHeight - 10);
            _directorCastWindow.OnResizing((int)_tabs.Size.X, (int)_tabs.Size.Y);
        }
       

        

        public new void Dispose()
        {
            
            //_mediator.Unsubscribe(this);
            base.Dispose();
        }
    }
}
