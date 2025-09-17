


namespace BlingoEngine.Director.Core.FileSystems
{
    public interface IDirFolderPicker
    {
        void PickFolder(Action<string> onPicked, string? currentFolder = null);
    }
}

