using Godot;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.LGodot;

namespace BlingoEngine.Director.LGodot.FileSystems;

public partial class GodotFilePicker : IDirFilePicker
{
    private readonly BlingoGodotRootNode _directorRoot;

    public GodotFilePicker(BlingoGodotRootNode directorRoot)
    {
        _directorRoot = directorRoot;
    }

    public void PickFile(Action<string> onPicked, string filter, string? currentFile = null)
    {
#if USE_WINDOWS_FEATURES
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new[] { filter },
            CurrentFile = currentFile
        };

        dialog.FileSelected += h => onPicked(h);
        _directorRoot.RootNode.AddChild(dialog);
        dialog.PopupCentered();
#else
        GD.PushWarning("File picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
    }
}

