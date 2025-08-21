using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using Godot;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.LGodot.Projects;

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

