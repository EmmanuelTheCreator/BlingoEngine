using System.IO;
using System.Linq;
using System.Text;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using ProjectorRays.CastMembers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class XmedReaderDirectoryTests
{
    private static Stream OpenFixture(string relativePath) => File.OpenRead(TestContextHarness.GetAssetPath(relativePath));

    [Fact]
    public void ReadHeaderDirectory_ExtractsKnownEntries()
    {
        using var stream = OpenFixture("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var reader = new XmedReader();

        var dir = reader.ReadHeaderDirectory(stream);

        dir.Should().NotBeNull();
        dir.Buffer.Should().NotBeEmpty();

        dir.FindEntry("0002").Should().NotBeNull();
        dir.FindEntry("0008").Should().NotBeNull();
    }

    [Fact]
    public void ReadTextBlock_UsesHeaderLength()
    {
        using var stream = OpenFixture("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var reader = new XmedReader();
        var dir = reader.ReadHeaderDirectory(stream);

        var (offset, length, data) = reader.ReadTextBlock(stream, dir);

        length.Should().Be(0x12A);
        data.Length.Should().Be(length);
        var text = Encoding.Latin1.GetString(data.Span);
        text.Should().StartWith("This text is red");
        text.Should().Contain("centered again");
    }

    [Fact]
    public void ReadFontTable_ReadsDeclaredFonts()
    {
        using var stream = OpenFixture("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var reader = new XmedReader();
        var dir = reader.ReadHeaderDirectory(stream);
        var fontEntry = dir.FindEntry("0008")!;

        using var bufferStream = new MemoryStream(dir.Buffer, writable: false);
        var fonts = reader.ReadFontTable(bufferStream, fontEntry);

        fonts.Should().NotBeEmpty();
        fonts.Select(f => f.Name).Should().Contain(new[] { "Arial", "Tahoma", "Terminal" });
    }

    [Fact]
    public void ReadRunMap_ReturnsRunLengths()
    {
        using var stream = OpenFixture("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var reader = new XmedReader();
        var dir = reader.ReadHeaderDirectory(stream);

        var runs = reader.ReadRunMap(stream, dir);

        runs.Count.Should().BeGreaterThanOrEqualTo(4);
        runs.Select(r => r.Length).Should().Contain(new ushort[] { 0x29, 0x1F, 0x273, 0x70 });
    }
}
