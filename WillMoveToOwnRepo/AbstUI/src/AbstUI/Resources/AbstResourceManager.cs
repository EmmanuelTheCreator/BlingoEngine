
namespace AbstUI.Resources
{
    public abstract class AbstResourceManager : IAbstResourceManager
    {
        public string ProjectFolder { get; set; } = "AbstData";

        public virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public virtual string? ReadTextFile(string fileName)
        {
            return File.Exists(fileName) ? File.ReadAllText(fileName) : null;
        }

        public virtual byte[]? ReadBytes(string fileName)
        {
            return File.Exists(fileName) ? File.ReadAllBytes(fileName) : null;
        }

        public virtual void StorageWrite<T>(string key, T data)
        {
            string saveData = Serialize(data); 
            if (!Directory.Exists(ProjectFolder))
                Directory.CreateDirectory(ProjectFolder);
            File.WriteAllText(CreateFilename(key), saveData);
        }


        protected virtual string CreateFilename(string key) => Path.Combine(ProjectFolder, key + ".json");

        public virtual T? StorageRead<T>(string key) 
        {
            var content = ReadTextFile(Path.Combine(ProjectFolder, key + ".json"));
            if (content != null)
                return Deserialize<T>(content); return default;
        }

        public string Serialize<T>(T data) 
        {
#if NET48
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
#else
            return System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
#endif
        }

        public T? Deserialize<T>(string content) 
        {
#if NET48
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
#else
            return System.Text.Json.JsonSerializer.Deserialize<T>(content);
#endif
        }
    }
}
