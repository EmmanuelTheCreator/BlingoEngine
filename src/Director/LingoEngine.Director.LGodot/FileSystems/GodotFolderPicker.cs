using Godot;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.LGodot;

namespace LingoEngine.Director.LGodot.FileSystems
{
    public partial class GodotFolderPicker : IDirFolderPicker
    {
        private readonly LingoGodotRootNode _directorRoot;

        public GodotFolderPicker(LingoGodotRootNode directorRoot)
        {
            _directorRoot = directorRoot;
        }

        public void PickFolder(Action<string> onPicked, string? currentFolder = null)
        {
#if USE_WINDOWS_FEATURES
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenDir,
            CurrentPath = currentFolder,
        };
        dialog.DirSelected += h =>
        {
            onPicked(h);
        };
        _directorRoot.RootNode.AddChild(dialog);
        dialog.PopupCentered();
#else
            GD.PushWarning("Executable folder picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
        }
    }
}
