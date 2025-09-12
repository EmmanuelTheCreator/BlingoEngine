using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ProjectorRays.DotNet.Test;

public class HeaderGenerator
{
    [Fact]
    public void GenerateHeaderFiles()
    {
        foreach (var file in GetFiles())
        {
            TestFileReader.ReadHeader(file);
        }
    }

    private static IEnumerable<string> GetFiles()
    {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "../../../../TestData");
        var dirs = Directory.EnumerateFiles(baseDir, "*.dir", SearchOption.AllDirectories);
        var casts = Directory.EnumerateFiles(baseDir, "*.cst", SearchOption.AllDirectories);
        return dirs.Concat(casts);
    }
}
