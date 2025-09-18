using System;
using System.IO;

namespace BlingoEngine.IO.Legacy.Tests.Helpers
{
    internal class TestFolder
    {
        public static string AssetPath(string relativePath)
        {
            var root = GetRepositoryRoot();
            var normalized = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            return Path.Combine(root,"Test", "TestData", "Legacy", normalized);
        }

        public static string GetRepositoryRoot()
        {
            var baseDirectory = AppContext.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", ".."));
        }
    }
}
