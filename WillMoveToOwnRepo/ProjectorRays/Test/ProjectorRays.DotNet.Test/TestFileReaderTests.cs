using System;
using System.IO;
using LingoEngine.Director.LGodot.Importer.TestData;
using Xunit;

namespace ProjectorRays.DotNet.Test;

public class TestFileReaderTests
{
    [Fact]
    public void ReadXmedCreatesHexDump()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xmed");
        File.WriteAllBytes(temp, XmedTestData.HalloDefault);
        try
        {
            var bytes = TestFileReader.ReadXmed(temp);
            Assert.Equal(XmedTestData.HalloDefault.Length, bytes.Length);
            Assert.True(File.Exists(Path.ChangeExtension(temp, ".xmed.txt")));
        }
        finally
        {
            File.Delete(temp);
            var dump = Path.ChangeExtension(temp, ".xmed.txt");
            if (File.Exists(dump)) File.Delete(dump);
        }
    }
}
