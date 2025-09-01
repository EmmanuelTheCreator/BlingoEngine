using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ProjectorRays.DotNet.Test.XMED;

public class XmedGenerator
{
    [Fact]
    public void GenerateXMEDFiles()
    {
        var allCastFiles = CastFiles();
        foreach (var file in allCastFiles)
        {
            if (!TestFileReader.XmedExists(file))
                TestFileReader.ReadXmed(file);
        }
    }
    private static IEnumerable<string> CastFiles()
    {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "../../../../TestData/Texts_Fields");
        var files = Directory.EnumerateFiles(baseDir, "*.cst",SearchOption.AllDirectories).ToList();
        return files;
    }
}
