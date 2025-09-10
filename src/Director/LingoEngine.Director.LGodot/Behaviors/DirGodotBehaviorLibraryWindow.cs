using System;
using LingoEngine.Director.Core.Behaviors;
using AbstUI.Windowing;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Behaviors;

internal partial class DirGodotBehaviorLibraryWindow : BaseGodotWindow, IDirFrameworkBehaviorLibraryWindow, IFrameworkFor<DirectorBehaviorLibraryWindow>
{
    private readonly DirectorBehaviorLibraryWindow _directorWindow;

    public DirGodotBehaviorLibraryWindow(DirectorBehaviorLibraryWindow directorWindow, IServiceProvider serviceProvider) : base("Behavior library",serviceProvider)
    {
        _directorWindow = directorWindow;
        Init(_directorWindow);
    }
}
