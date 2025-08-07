


namespace LingoEngine.Director.Core.FileSystems
{
    public interface IDirExecutableFilePicker
    {
        void PickExecutable(Action<string> onPicked);
    } 
}
