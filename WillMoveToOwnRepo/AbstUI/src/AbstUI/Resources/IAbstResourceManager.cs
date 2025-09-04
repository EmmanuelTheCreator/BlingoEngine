namespace AbstUI.Resources
{
    public interface IAbstResourceManager
    {
        string? ReadTextFile(string fileName);
        byte[]? ReadBytes(string fileName);
    }
}
