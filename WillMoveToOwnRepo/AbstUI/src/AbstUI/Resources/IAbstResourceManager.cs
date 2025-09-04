namespace AbstUI.Resources
{
    public interface IAbstResourceManager
    {
        bool FileExists(string fileName);
        string? ReadTextFile(string fileName);
        byte[]? ReadBytes(string fileName);
    }
}
