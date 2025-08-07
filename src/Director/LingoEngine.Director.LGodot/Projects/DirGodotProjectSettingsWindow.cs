using Godot;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.LGodot.Gfx;
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
        Size = new Vector2(420, 200);
        CustomMinimumSize = Size;

        var root = directorWindow.RootPanel.Framework<LingoGodotWrapPanel>();
        root.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(root);
    }
}

