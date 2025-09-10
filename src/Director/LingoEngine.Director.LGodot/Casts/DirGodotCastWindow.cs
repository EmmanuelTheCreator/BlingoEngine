using LingoEngine.Director.Core.Casts;
using LingoEngine.Members;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDirFrameworkCastWindow, IFrameworkFor<DirectorCastWindow>
    {
        private readonly DirectorCastWindow _directorCastWindow;
        internal ILingoMember? SelectedMember => _directorCastWindow.SelectedMember;

        public DirGodotCastWindow(DirectorCastWindow directorCastWindow, IServiceProvider serviceProvider)
            : base("Cast", serviceProvider)
        {
            _directorCastWindow = directorCastWindow;
            Init(_directorCastWindow);
        }
    }
}
