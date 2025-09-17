using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstUI.Primitives;
using AbstUI.Resources;
using AbstUI.Texts;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Texts;
using BlingoEngine.Tools;
using BlingoEngine.Core;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Tests.Casts;
using Xunit;
using System.Threading.Tasks;

namespace BlingoEngine.Tests;

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
        Assert.Equal(BlingoMemberType.Text, row.Type);
        Assert.Equal("Sample", row.Name);
        Assert.Equal(1, row.RegPoint.X);
        Assert.Equal(2, row.RegPoint.Y);
        Assert.Equal("sample.txt", row.FileName);
    }
    [Fact]
    public async Task ImportInCastFromCsvFile_PrefersMarkdown()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var csvPath = Path.Combine(tempDir, "cast.csv");
        var baseFile = Path.Combine(tempDir, "sample.txt");
        var mdPath = Path.ChangeExtension(baseFile, ".md");
        var rtfPath = Path.ChangeExtension(baseFile, ".rtf");

        File.WriteAllText(csvPath, "Number,Type,Name,Registration Point,Filename\n1,Text,Sample,\"(0, 0)\",sample.txt");
        File.WriteAllText(mdPath, "md text");
        File.WriteAllText(rtfPath, "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 rtf}} ");

        var cast = new DummyCast();
        var importer = new CsvImporter(new TestResourceManager());

        await importer.ImportInCastFromCsvFileAsync(cast, csvPath);

        var member = Assert.IsType<DummyTextMember>(cast.LastAddedMember);
        Assert.Equal("md text", member.Text);
        Assert.False(member.LoadFileCalled);
    }

#if DEBUG
    [Fact]
    public async Task ImportInCastFromCsvFile_CreatesMarkdownWhenReadingRtf()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var csvPath = Path.Combine(tempDir, "cast.csv");
        var baseFile = Path.Combine(tempDir, "sample.txt");
        var mdPath = Path.ChangeExtension(baseFile, ".md");
        var rtfPath = Path.ChangeExtension(baseFile, ".rtf");

        File.WriteAllText(csvPath, "Number,Type,Name,Registration Point,Filename\n1,Text,Sample,\"(0, 0)\",sample.txt");
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 Hello}}";
        File.WriteAllText(rtfPath, rtf);

        var cast = new DummyCast();
        var importer = new CsvImporter(new TestResourceManager());

        await importer.ImportInCastFromCsvFileAsync(cast, csvPath);

        var member = Assert.IsType<DummyTextMember>(cast.LastAddedMember);
        Assert.Contains("{{PARA:0}}Hello", member.Text);
        Assert.True(File.Exists(mdPath));
    }
#endif

    private class TestResourceManager : AbstResourceManager
    {
        public override bool FileExists(string fileName) => File.Exists(fileName);
        public override string? ReadTextFile(string fileName) => File.Exists(fileName) ? File.ReadAllText(fileName) : null;

        public override byte[]? ReadBytes(string fileName) => File.Exists(fileName) ? File.ReadAllBytes(fileName) : null;
    }
}

