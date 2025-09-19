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

    [Theory]
    [InlineData("Texts_Fields/Text_Hallo_13.xmed.bin", BlXmedAlignment.Center)]
    [InlineData("Texts_Fields/Text_Hallo_textAlignLeft_13.xmed.bin", BlXmedAlignment.Left)]
    [InlineData("Texts_Fields/Text_Hallo_textAlignRight_13.xmed.bin", BlXmedAlignment.Right)]
    public void ReadSingleStyleChunk_ParsesAlignment(string relativePath, BlXmedAlignment expectedAlignment)
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath(relativePath));

        var doc = new BlXmedReader().Read(data);

        doc.Text.Should().Be("Hallo");
        doc.Runs.Should().ContainSingle();

        doc.Styles.Should().NotBeEmpty();
        doc.Styles[0].Alignment.Should().Be(expectedAlignment);
        doc.Styles.Select(style => style.Alignment).Should().Contain(expectedAlignment);
    }

    [Fact]
    public void ReadSingleStyleChunk_DecodesStyleFlagBits()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_13.xmed.bin");
        var original = File.ReadAllBytes(path);

        var allFlags = (byte[])original.Clone();
        allFlags[0x1C] = 0xFF;
        var allDoc = new BlXmedReader().Read(allFlags);

        var allStyle = allDoc.Styles[0];
        allStyle.Bold.Should().BeTrue();
        allStyle.Italic.Should().BeTrue();
        allStyle.Underline.Should().BeTrue();
        allStyle.Strikeout.Should().BeTrue();
        allStyle.Subscript.Should().BeTrue();
        allStyle.Superscript.Should().BeTrue();
        allStyle.TabbedField.Should().BeTrue();
        allStyle.EditableField.Should().BeTrue();
        allStyle.Locked.Should().BeFalse();

        var noFlags = (byte[])original.Clone();
        noFlags[0x1C] = 0x00;
        var noDoc = new BlXmedReader().Read(noFlags);

        var noStyle = noDoc.Styles[0];
        noStyle.Bold.Should().BeFalse();
        noStyle.Italic.Should().BeFalse();
        noStyle.Underline.Should().BeFalse();
        noStyle.Strikeout.Should().BeFalse();
        noStyle.Subscript.Should().BeFalse();
        noStyle.Superscript.Should().BeFalse();
        noStyle.TabbedField.Should().BeFalse();
        noStyle.EditableField.Should().BeFalse();
        noStyle.Locked.Should().BeTrue();
    }

    [Fact]
    public void ReadSingleStyleChunk_DecodesAlignmentFlagBits()
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_textAlignLeft_13.xmed.bin");
        var original = File.ReadAllBytes(path);

        var withWrapAndTabs = (byte[])original.Clone();
        withWrapAndTabs[0x1D] = 0x1A; // Left + wrap disabled + tab bits
        var doc = new BlXmedReader().Read(withWrapAndTabs);

        var style = doc.Styles[0];
        style.AlignmentRaw.Should().Be(0x1A);
        style.AlignmentFromFlags.Should().Be(BlXmedAlignment.Left);
        style.WrapOff.Should().BeTrue();
        style.HasTabs.Should().BeTrue();
        style.Alignment.Should().Be(BlXmedAlignment.Left);
        style.AlignmentMarker.Should().Be((byte)0x3B);

        var justifyData = (byte[])original.Clone();
        justifyData[0x1D] = 0x03; // Justified alignment, no modifiers
        var justifyDoc = new BlXmedReader().Read(justifyData);

        var justifyStyle = justifyDoc.Styles[0];
        justifyStyle.AlignmentRaw.Should().Be(0x03);
        justifyStyle.AlignmentFromFlags.Should().Be(BlXmedAlignment.Justify);
        justifyStyle.WrapOff.Should().BeFalse();
        justifyStyle.HasTabs.Should().BeFalse();
    }

    [Fact]
    public void Read_Text12Chars_RunMatchesExpectedText()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_12chars_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.Start.Should().Be(0);
        run.Length.Should().Be(12);
        run.Text.Should().Be("Hallo12Chars");
    }

    [Fact]
    public void Read_Text3Paragraphs_PreservesAllRunSegments()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_3_Paragraps_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        var expectedRuns = new[]
        {
            "My first paragraph centered with all 0\rParagraph with align Left, Margin Left 4, Margin Right 5, First Inden",
            " spacing ",
            " ",
            " F",
            " spa"
        };

        doc.Runs.Should().HaveSameCount(expectedRuns);
        doc.Runs.Select(run => run.Text).Should().Equal(expectedRuns);
        string.Concat(doc.Runs.Select(run => run.Text)).Should().Be(doc.Text);
    }

    [Fact]
    public void Read_TextHallo_RunMatchesExpectedText()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].Text.Should().Be("Hallo");
    }

    [Theory]
    [InlineData("Texts_Fields/Text_Hallo_2line_linespace_16_13.xmed.bin")]
    [InlineData("Texts_Fields/Text_Hallo_2line_linespace_30_13.xmed.bin")]
    [InlineData("Texts_Fields/Text_Hallo_2line_linespace_Default_13.xmed.bin")]
    public void Read_TextHalloTwoLineSamples_RunContainsCarriageReturn(string relativePath)
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath(relativePath));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.Text.Should().Be("Hallo\rline2");
        run.Length.Should().Be(11);
    }

    [Fact]
    public void Read_TextHalloChangedColor_ReportsMultipleColorIndices()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_changed_color_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].Text.Should().Be("Hallo");

        doc.Styles
            .Select(style => style.ColorIndex)
            .Distinct()
            .Should()
            .HaveCountGreaterThan(1);
    }

    [Fact]
    public void Read_TextHalloEditableTrue_ExposesLeftAlignmentStyle()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_editable_true_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();

        var run = doc.Runs[0];
        run.Text.Should().Be("Hallo");
        run.FontName.Should().Be("Vivaldi");

        doc.Styles
            .Select(style => style.Alignment)
            .Should()
            .Contain(BlXmedAlignment.Left);
    }

    [Fact]
    public void Read_TextHalloFontVivaldi_RunUsesDeclaredFont()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_font_Vivaldi_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].FontName.Should().Be("Vivaldi");
        doc.Runs[0].Text.Should().Be("Hallo");
    }

    [Fact]
    public void Read_TextHalloFontSize14_RunCarriesFontSize()
    {
        var data = File.ReadAllBytes(TestContextHarness.GetAssetPath("Texts_Fields/Text_Hallo_fontsize14_13.xmed.bin"));

        var doc = new BlXmedReader().Read(data);

        doc.Runs.Should().ContainSingle();
        doc.Runs[0].FontSize.Should().Be((ushort)14);
        doc.Runs[0].Text.Should().Be("Hallo");
    }
}
