


namespace LingoEngine.Director.Core.FileSystems;

public interface IDirFilePicker
{
    void PickFile(Action<string> onPicked, string filter, string? currentFile = null);
}
