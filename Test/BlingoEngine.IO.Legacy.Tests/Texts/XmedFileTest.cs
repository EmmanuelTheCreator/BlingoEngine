using System.IO;
using System.Linq;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using ProjectorRays.CastMembers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

// Each test targets a specific XMED sample under Texts_Fields.
public class XmedFileTest
{
    [Fact]
    public void Text_Hallo_tab_true_file_should_report_tabs()
    {
        var document = ReadDocument("Text_Hallo_tab_true_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        document.Styles.Should().NotBeEmpty();
        document.Styles[0].HasTabs.Should().BeTrue();
    }

    [Fact]
    public void Text_Hallo_multifont_file_should_list_multiple_fonts()
    {
        var document = ReadDocument("Text_Hallo_multifont_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        var fonts = document.Styles
            .Select(s => s.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();
        fonts.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Text_Hallo_changed_color_file_should_report_color_index()
    {
        var document = ReadDocument("Text_Hallo_changed_color_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        document.Styles.Should().Contain(s => s.ColorIndex != 0);
    }

    [Fact]
    public void Text_Hallo_text_transform_all_on_file_should_flag_all_styles()
    {
        var document = ReadDocument("Text_Hallo_text_transform_all_on_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        document.Styles.Should().Contain(s => s.Bold && s.Italic && s.Underline && s.Strikeout);
    }

    [Fact]
    public void Text_Single_Line_Multi_Style_file_should_read_long_text_and_runs()
    {
        var document = ReadDocument("Text_Single_Line_Multi_Style_13.xmed.bin");

        document.Text.Should().Be("This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again");
        document.RunMap.Should().NotBeEmpty();
        document.Styles.Should().HaveCountGreaterThan(1);
    }

    private static XmedDocument ReadDocument(string fileName)
    {
        var path = TestContextHarness.GetAssetPath($"Texts_Fields/{fileName}");
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader();
        return reader.Read(bytes);
    }
}
