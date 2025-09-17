using System.IO;

namespace BlingoEngine.IO;

internal static class MediaFileNameHelper
{
    public static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name;
    }
}

