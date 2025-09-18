using System.IO;
using System.Linq;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using ProjectorRays.CastMembers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedReaderTests
{
    [Fact]
    public void ReadSingleStyleChunk_ParsesFontSizeAndText()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_fontsize14_13.xmed.bin");
        var data = File.ReadAllBytes(path);

        var doc = new BlXmedReader().Read(data);

        doc.Text.Should().Be("Hallo");
        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.Text.Should().Be("Hallo");
        run.FontSize.Should().Be((ushort)14);

        doc.Styles.Should().NotBeEmpty();
        doc.Styles[0].FontSize.Should().Be((ushort)14);
    }

    [Fact]
    public void ReadSingleLineMultiStyleChunk_ParsesTextAndMetadata()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Single_Line_Multi_Style_13.xmed.bin");
        var data = File.ReadAllBytes(path);

        var doc = new BlXmedReader().Read(data);

        const string expectedText = "This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again";
        doc.Text.Should().Be(expectedText);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].Text.Should().Be(expectedText);

        var styleFonts = doc.Styles
            .Select(style => style.FontName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        styleFonts.Should().Contain(new[] { "Arcade *", "arial", "Tahoma", "Terminal" });

        doc.MapEntries.Should().NotBeEmpty();
        doc.MapEntries
            .Select(entry => entry.TextLength)
            .Should().Contain(new ushort[] { 37, 48, 13 });
    }

    [Fact]
    public void ReadMultiLineMultiStyleChunk_PreservesParagraphs()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var data = File.ReadAllBytes(path);

        var doc = new BlXmedReader().Read(data);

        const string expectedText = "This text is red, Arial,12px, centered\rThe text is yellow, Tahoma, 9px, left aligned, bold, italic, underline\rThe text is green, font Terminal, 18px, left aligned, with spacing of 39\rThe text is orange, Tahoma, 9px, left aligned, bold, italic, underline\rThis text is red, Arial,12px, centered again";
        doc.Text.Should().Be(expectedText);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].Text.Should().Be(expectedText);

        doc.MapEntries.Should().NotBeEmpty();
        doc.MapEntries
            .Select(entry => entry.TextLength)
            .Should().Contain(new ushort[] { 130, 29, 70, 15 });
    }

    [Fact]
    public void ReadMultiLineChunk_PreservesCarriageReturns()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_multiLine_13.xmed.bin");
        var data = File.ReadAllBytes(path);

        var doc = new BlXmedReader().Read(data);

        const string expectedText = "Hallo\rmulti line\ris longer\rYES!";
        doc.Text.Should().Be(expectedText);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].Text.Should().Be(expectedText);
    }

    [Theory]
    [InlineData("Texts_Fields/Text_Hallo2_13.xmed.bin", "Hallo2")]
    [InlineData("Texts_Fields/Text_12chars_13.xmed.bin", "Hallo12Chars")]
    public void ReadSingleStyleChunk_ParsesHexAndDecimalRunLengths(string relativePath, string expectedText)
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath(relativePath));

        var doc = new BlXmedReader().Read(data);

        doc.Text.Should().Be(expectedText);
        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.Text.Should().Be(expectedText);
        run.Length.Should().Be(expectedText.Length);
    }

    [Theory]
    [InlineData("Texts_Fields/Text_Hallo_font_Vivaldi_13.xmed.bin", "Vivaldi")]
    [InlineData("Texts_Fields/Text_Hallo_multifont_13.xmed.bin", "Trajan Pro")]
    [InlineData("Texts_Fields/Text_Hallo_editable_true_13.xmed.bin", "Vivaldi")]
    public void ReadSingleStyleChunk_UsesDeclaredFontNames(string relativePath, string expectedFont)
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath(relativePath));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.FontName.Should().Be(expectedFont);
        run.Text.Should().Be(doc.Text);

        doc.Styles
            .Select(style => style.FontName)
            .Should().Contain(expectedFont);
    }
}
