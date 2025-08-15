using AbstUI.LGodot.Components;
using Godot;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.LGodot.Windowing;

namespace LingoEngine.Director.LGodot.Projects;

/// <summary>
/// Godot wrapper for <see cref="DirectorProjectSettingsWindow"/>.
/// </summary>
internal partial class DirGodotProjectSettingsWindow : BaseGodotWindow, IDirFrameworkProjectSettingsWindow
{
    public DirGodotProjectSettingsWindow(
        DirectorProjectSettingsWindow directorWindow,
        IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ProjectSettingsWindow, "Project Settings", windowManager)
    {
        directorWindow.Init(this);
        Size = new Vector2(directorWindow.Width, directorWindow.Height);
        CustomMinimumSize = Size;

        var root = directorWindow.RootPanel.Framework<AbstGodotWrapPanel>();
        root.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(root);
    }
}

