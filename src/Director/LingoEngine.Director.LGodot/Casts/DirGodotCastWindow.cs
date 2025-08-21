using LingoEngine.Director.Core.Casts;
using Godot;
using LingoEngine.Members;
using AbstUI.LGodot.Components;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDirFrameworkCastWindow, IFrameworkFor<DirectorCastWindow>
    {
        private readonly DirectorCastWindow _directorCastWindow;
        private readonly AbstGodotTabContainer _tabs;
        internal ILingoMember? SelectedMember => _directorCastWindow.SelectedMember;

        public DirGodotCastWindow(DirectorCastWindow directorCastWindow, IServiceProvider serviceProvider)
            : base("Cast", serviceProvider)
        {
            _directorCastWindow = directorCastWindow;
            Init(_directorCastWindow);
           
            //_mediator.Subscribe(this);
            //var size = new Vector2(directorCastWindow.Width, directorCastWindow.Height);
            //Size = size;
            //CustomMinimumSize = size;// new Vector2(directorCastWindow.MinimumWidth, directorCastWindow.MinimumHeight);

            _tabs = _directorCastWindow.TabContainer.Framework<AbstGodotTabContainer>();
            _tabs.Position = new Vector2(0, TitleBarHeight);
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            AddChild(_tabs);
           
        }

        public override void _Ready()
        {
            base._Ready();
            UpdateSize(Size, true);
        }


        protected override void OnResizing(Vector2 size)
        {
            if (size.X < _directorCastWindow.MinimumWidth)
            {
                Size = new Vector2(_directorCastWindow.MinimumWidth, size.Y);
                CustomMinimumSize = Size;
            }
            if (size.X < _directorCastWindow.MinimumHeight + TitleBarHeight + 10)
            {
                Size = new Vector2(_directorCastWindow.MinimumHeight + TitleBarHeight + 10, size.Y);
                CustomMinimumSize = Size;
            }
            base.OnResizing(size);
            UpdateSize(size);
        }

        private void UpdateSize(Vector2 size, bool firstLoad = false)
        {
            _directorCastWindow.ResizeFromFW(firstLoad,(int)Size.X, (int)Size.Y - TitleBarHeight - 10);
        }
       

        

        public new void Dispose()
        {
            
            //_mediator.Unsubscribe(this);
            base.Dispose();
        }
    }
}
