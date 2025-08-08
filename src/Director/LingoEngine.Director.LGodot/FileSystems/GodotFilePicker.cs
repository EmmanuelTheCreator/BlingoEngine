using Godot;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.LGodot;

namespace LingoEngine.Director.LGodot.FileSystems;

public partial class GodotFilePicker : IDirFilePicker
{
    private readonly LingoGodotRootNode _directorRoot;

    public GodotFilePicker(LingoGodotRootNode directorRoot)
    {
        _directorRoot = directorRoot;
    }

    public void PickFile(Action<string> onPicked, string filter)
    {
#if USE_WINDOWS_FEATURES
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new[] { filter }
        };

        dialog.FileSelected += h => onPicked(h);
        _directorRoot.RootNode.AddChild(dialog);
        dialog.PopupCentered();
#else
        GD.PushWarning("File picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
    }
}
