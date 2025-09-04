using System.IO;
using System.Linq;
using LingoEngine.Members;
using LingoEngine.Tools;
using Xunit;
using AbstUI.Resources;
namespace LingoEngine.Tests;
public class CsvImporterTests
{
    [Fact]
    public void ImportCsvCastFile_ReturnsExpectedRow()
    {
        var csvContent = "Number,Type,Name,Registration Point,Filename\n" +
                         "1,Text,Sample,\"(1, 2)\",sample.txt";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        var importer = new CsvImporter(new TestResourceManager());
        var rows = importer.ImportCsvCastFile(tempFile);

        var row = Assert.Single(rows);
        Assert.Equal(1, row.Number);
        Assert.Equal(LingoMemberType.Text, row.Type);
        Assert.Equal("Sample", row.Name);
        Assert.Equal(1, row.RegPoint.X);
        Assert.Equal(2, row.RegPoint.Y);
        Assert.Equal("sample.txt", row.FileName);
    }
    private class TestResourceManager : IAbstResourceManager
    {
        public bool FileExists(string fileName) => File.Exists(fileName);
        public string? ReadTextFile(string fileName) => File.Exists(fileName) ? File.ReadAllText(fileName) : null;

        public byte[]? ReadBytes(string fileName) => File.Exists(fileName) ? File.ReadAllBytes(fileName) : null;
    }
}
