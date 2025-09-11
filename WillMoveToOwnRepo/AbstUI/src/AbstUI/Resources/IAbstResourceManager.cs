
namespace AbstUI.Resources
{
    public interface IAbstResourceManager
    {
        string ProjectFolder { get; set; }
        bool FileExists(string fileName);
        string? ReadTextFile(string fileName);
        byte[]? ReadBytes(string fileName);
        void StorageWrite<T>(string key, T data);
        T? StorageRead<T>(string key);
        string Serialize<T>(T data);
        T? Deserialize<T>(string content);
    }
}
