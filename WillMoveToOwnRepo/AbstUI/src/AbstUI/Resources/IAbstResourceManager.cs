
using System.Threading.Tasks;

namespace AbstUI.Resources
{
    public interface IAbstResourceManager
    {
        string ProjectFolder { get; set; }
        bool FileExists(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        string? ReadTextFile(string fileName);
        Task<string?> ReadTextFileAsync(string fileName);
        byte[]? ReadBytes(string fileName);
        Task<byte[]?> ReadBytesAsync(string fileName);
        void StorageWrite<T>(string key, T data);
        Task StorageWriteAsync<T>(string key, T data);
        T? StorageRead<T>(string key);
        Task<T?> StorageReadAsync<T>(string key);
        string Serialize<T>(T data);
        T? Deserialize<T>(string content);
    }
}
