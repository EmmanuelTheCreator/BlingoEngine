using System.IO;
using AbstUI.Resources;

namespace AbstUI.SDL2.Resources
{
    public class SdlResourceManager : IAbstResourceManager
    {
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
