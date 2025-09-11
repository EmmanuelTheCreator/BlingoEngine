using AbstUI.LGodot.Helpers;
using AbstUI.Resources;
using Godot;

namespace AbstUI.LGodot.Resources
{
    public class AbstGodotResourceManager : AbstResourceManager
    {
        public override bool FileExists(string fileName)
        {
            var filePath = GodotHelper.EnsureGodotUrl(fileName);
            return Godot.FileAccess.FileExists(filePath);
        }

        public override string? ReadTextFile(string fileName) => GodotHelper.ReadFile(fileName);

        public override byte[]? ReadBytes(string fileName)
        {
            var filePath = GodotHelper.EnsureGodotUrl(fileName);
            var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
                return null;
            return file.GetBuffer((long)file.GetLength());
        }

        public override void StorageWrite<T>(string key, T data)
        {
            var relativePath = CreateFilename(key); // e.g., "settings/userprefs.json"
            // Ensure directory exists (e.g., "data/settings/")
            var dirPath = System.IO.Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? "";
            if (!string.IsNullOrEmpty(dirPath))
            {
                using var root = DirAccess.Open("user://");
                if (root == null) return;
                foreach (var part in dirPath.Split('/'))
                {
                    if (string.IsNullOrEmpty(part)) continue;
                    if (!root.DirExists(part)) root.MakeDir(part);
                    root.ChangeDir(part);
                }
            }

            var fullPath = $"user://{relativePath}";
            using var f = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Write);
            if (f == null)
            {
                GD.PushError($"Failed to open for write: {fullPath} (err {(Error)Godot.FileAccess.GetOpenError()})");
                return;
            }

            f.StoreString(Serialize(data));   // UTF-8 by default
        }
        public override T? StorageRead<T>(string key) where T : default
        {
            var relativePath = CreateFilename(key); // e.g., "settings/userprefs.json"
            var fullPath = $"user://{relativePath}";
            if (!Godot.FileAccess.FileExists(fullPath)) return default;
            var dataString = Godot.FileAccess.GetFileAsString(fullPath);
            var data = Deserialize<T>(dataString);
            return data;
        }
    }
}
