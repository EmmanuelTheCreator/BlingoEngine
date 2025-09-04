using AbstUI.Resources;
using AbstUI.LGodot.Helpers;

namespace AbstUI.LGodot.Resources
{
    public class AbstGodotResourceManager : IAbstResourceManager
    {
        public string? ReadTextFile(string fileName) => GodotHelper.ReadFile(fileName);

        public byte[]? ReadBytes(string fileName)
        {
            var filePath = GodotHelper.EnsureGodotUrl(fileName);
            var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
                return null;
            return file.GetBuffer((long)file.GetLength());
        }
    }
}
