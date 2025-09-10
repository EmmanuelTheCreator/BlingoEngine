using System;
using LingoEngine.Director.Core.Behaviors;
using AbstUI.Windowing;

namespace LingoEngine.Director.LGodot.Behaviors;

internal partial class DirGodotBehaviorLibraryWindow : BaseGodotWindow, IDirFrameworkBehaviorLibraryWindow, IFrameworkFor<DirectorBehaviorLibraryWindow>
{
    private readonly DirectorBehaviorLibraryWindow _directorWindow;

    public DirGodotBehaviorLibraryWindow(DirectorBehaviorLibraryWindow directorWindow, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _directorWindow = directorWindow;
        Init(_directorWindow);
    }
}
