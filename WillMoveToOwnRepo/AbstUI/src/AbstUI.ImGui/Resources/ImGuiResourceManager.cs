using System.IO;
using AbstUI.Resources;

namespace AbstUI.ImGui.Resources
{
    public class ImGuiResourceManager : IAbstResourceManager
    {
        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public string? ReadTextFile(string fileName)
        {
            return File.Exists(fileName) ? File.ReadAllText(fileName) : null;
        }

        public byte[]? ReadBytes(string fileName)
        {
            return File.Exists(fileName) ? File.ReadAllBytes(fileName) : null;
        }
    }
}
