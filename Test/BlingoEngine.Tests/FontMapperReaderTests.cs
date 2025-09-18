using System.IO;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.IO;
using FluentAssertions;

namespace BlingoEngine.Tests;

public class FontMapperReaderTests
{
    private static string GetProjectRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        var root = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        return root;
    }

    [Fact]
    public void ReadFromFile_ParsesFontAndKeyMappings()
    {
        var root = GetProjectRoot();
        var path = Path.Combine(root, "WillMoveToOwnRepo", "ProjectorRays", "src", "ProjectorRays.DotNet", "fontmaps", "fontmap_D6.txt");
        File.Exists(path).Should().BeTrue($"Expected font map file at {path}");

        var document = FontMapperReader.ReadFromFile(path);

        document.FontMappings.Should().NotBeEmpty();
        document.InputKeyMappings.Should().NotBeEmpty();

        var macTimes = document.FontMappings.FirstOrDefault(m =>
            m.SourcePlatform == ARuntimePlatform.Mac && m.SourceFontName == "Times" && m.TargetPlatform == ARuntimePlatform.Win);
        macTimes.Should().NotBeNull();
        macTimes!.TargetFontName.Should().Be("Times New Roman");
        macTimes.MapCharacters.Should().BeTrue();
        macTimes.SizeMappings.Should().ContainKey(14);
        macTimes.SizeMappings[14].Should().Be(12);

        var macSymbol = document.FontMappings.FirstOrDefault(m =>
            m.SourcePlatform == ARuntimePlatform.Mac && m.SourceFontName == "Symbol");
        macSymbol.Should().NotBeNull();
        macSymbol!.MapCharacters.Should().BeFalse();

        var macToWin = document.InputKeyMappings.FirstOrDefault(m =>
            m.SourcePlatform == ARuntimePlatform.Mac && m.TargetPlatform == ARuntimePlatform.Win);
        macToWin.Should().NotBeNull();
        macToWin!.Keys.Should().ContainKey(128);
        macToWin.Keys[128].Should().Be(196);
    }

    [Theory]
    [InlineData(600, "fontmap_D6.txt")]
    [InlineData(750, "fontmap_D7.txt")]
    [InlineData(850, "fontmap_D8_5.txt")]
    [InlineData(900, "fontmap_D9.txt")]
    [InlineData(1000, "fontmap_D10.txt")]
    [InlineData(1100, "fontmap_D11.txt")]
    [InlineData(1150, "fontmap_D11_5.txt")]
    public void GetFontMapFileName_ResolvesExpectedFile(int version, string expected)
    {
        FontMapperReader.GetFontMapFileName(version).Should().Be(expected);
    }
}
