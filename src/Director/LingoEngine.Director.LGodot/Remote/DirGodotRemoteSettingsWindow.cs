using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using Godot;
using LingoEngine.Director.Core.Remote;

namespace LingoEngine.Director.LGodot.Remote;

/// <summary>Godot wrapper for <see cref="DirectorRemoteSettingsWindow"/>.</summary>
internal partial class DirGodotRemoteSettingsWindow : BaseGodotWindow, IDirFrameworkRemoteSettingsWindow, IFrameworkFor<DirectorRemoteSettingsWindow>
{
    public DirGodotRemoteSettingsWindow(DirectorRemoteSettingsWindow directorWindow, IServiceProvider serviceProvider)
        : base("Remote Settings", serviceProvider)
    {
        Init(directorWindow);
        var root = directorWindow.RootPanel.Framework<AbstGodotWrapPanel>();
        root.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(root);
    }
}
