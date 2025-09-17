using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using Godot;
using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.UI;

namespace BlingoEngine.Director.LGodot.Projects;

/// <summary>
/// Godot wrapper for <see cref="DirectorProjectSettingsWindow"/>.
/// </summary>
internal partial class DirGodotProjectSettingsWindow : BaseGodotWindow, IDirFrameworkProjectSettingsWindow, IFrameworkFor<DirectorProjectSettingsWindow>
{
    public DirGodotProjectSettingsWindow(
        DirectorProjectSettingsWindow directorWindow,
        IServiceProvider serviceProvider)
        : base("Project Settings", serviceProvider)
    {
        Init(directorWindow);

        var root = directorWindow.RootPanel.Framework<AbstGodotWrapPanel>();
        root.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(root);
    }
}


