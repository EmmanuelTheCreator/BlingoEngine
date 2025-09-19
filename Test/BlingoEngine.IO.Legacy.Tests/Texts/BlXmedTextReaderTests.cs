using System.IO;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using ProjectorRays.CastMembers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTextReaderTests
{
    [Fact]
    public void Read_SingleRunText_ParsesHeaderAndText()
    {
        var document = ReadDocument("Texts_Fields/Text_Hallo_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        document.Runs.Should().ContainSingle();

        var run = document.Runs[0];
        run.Start.Should().Be(0);
        run.Length.Should().Be(5);
        run.Text.Should().Be("Hallo");
    }

    [Fact]
    public void Read_DecodesStyleFlags()
    {
        var italic = ReadDocument("Texts_Fields/Text_Hallo_italic_13.xmed.bin");
        italic.Styles.Should().Contain(s => s.Italic);

        var underline = ReadDocument("Texts_Fields/Text_Hallo_underline_13.xmed.bin");
        underline.Styles.Should().Contain(s => s.Underline);
    }

    [Fact]
    public void Read_ParsesStyleDescriptorsForMultifont()
    {
        var document = ReadDocument("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");

        document.Text.Should().Contain("This text is red");
        document.RunMap.Should().NotBeEmpty();
        document.Styles.Should().HaveCountGreaterThan(1);
    }

    private static XmedDocument ReadDocument(string asset)
    {
        var path = TestContextHarness.GetAssetPath(asset);
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader();
        return reader.Read(bytes);
    }
}
