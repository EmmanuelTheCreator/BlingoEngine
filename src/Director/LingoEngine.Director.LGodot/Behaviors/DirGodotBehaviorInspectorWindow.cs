using System;
using LingoEngine.Director.Core.Behaviors;
using AbstUI.Windowing;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Behaviors;

internal partial class DirGodotBehaviorInspectorWindow : BaseGodotWindow, IDirFrameworkBehaviorInspectorWindow, IFrameworkFor<DirBehaviorInspectorWindow>
{
    private readonly DirBehaviorInspectorWindow _directorWindow;

    public DirGodotBehaviorInspectorWindow(DirBehaviorInspectorWindow directorWindow, IServiceProvider serviceProvider) : base("Behavior library",serviceProvider)
    {
        _directorWindow = directorWindow;
        Init(_directorWindow);
    }
}
