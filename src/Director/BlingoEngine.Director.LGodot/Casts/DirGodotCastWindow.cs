using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Members;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace BlingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDirFrameworkCastWindow, IFrameworkFor<DirectorCastWindow>
    {
        private readonly DirectorCastWindow _directorCastWindow;
        internal IBlingoMember? SelectedMember => _directorCastWindow.SelectedMember;

        public DirGodotCastWindow(DirectorCastWindow directorCastWindow, IServiceProvider serviceProvider)
            : base("Cast", serviceProvider)
        {
            _directorCastWindow = directorCastWindow;
            Init(_directorCastWindow);
        }
    }
}

