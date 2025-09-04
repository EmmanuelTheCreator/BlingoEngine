using System.IO;
using AbstUI.Resources;

namespace AbstUI.LUnity.Resources
{
    public class UnityResourceManager : IAbstResourceManager
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
