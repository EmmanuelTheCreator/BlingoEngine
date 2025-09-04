using System.Linq;
using AbstUI.Texts;
using LingoEngine.Tools;
using Xunit;
namespace LingoEngine.Tests;
public class RtfToMarkdownTests
{
    [Fact]
    public void Convert_ReturnsStyledMarkdown_SingleLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 plain}{\\b\\f0\\fs24\\cf1 bold}{\\i\\f0\\fs24\\cf1 italic}{\\ul\\f0\\fs24\\cf1 underline}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:left}}plain**bold***italic*__underline__", data.Markdown);
        Assert.Equal(new[]
        {
            (false, false, false),
            (true, false, false),
            (false, true, false),
            (false, false, true)
        }, data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)));
    }

    [Fact]
    public void Convert_ReturnsStyledMarkdown_MultiLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue255;}{\\f0\\fs24\\cf1 line1\\par}{\\i\\f0\\fs24\\cf2 line2}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:left}}line1\n{{COLOR:#0000FF}}*line2*";
        Assert.Equal(expected, data.Markdown);
        Assert.Equal(new[]
        {
            (false, false, false),
            (false, true, false)
        }, data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)));
        Assert.True(data.Segments[0].IsParagraph);
        Assert.False(data.Segments[1].IsParagraph);
        Assert.Equal("line1\nline2", data.PlainText);
    }

    [Fact]
    public void Convert_ParsesRightAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qr{\\f0\\fs24\\cf1 right}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:right}}right", data.Markdown);
        Assert.Single(data.Segments);
        Assert.Equal(AbstTextAlignment.Right, data.Segments[0].Alignment);
    }

    [Fact]
    public void Convert_ParsesCenterAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qc{\\f0\\fs24\\cf1 center}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:center}}center", data.Markdown);
        Assert.Single(data.Segments);
        Assert.Equal(AbstTextAlignment.Center, data.Segments[0].Alignment);
    }

    [Fact]
    public void Convert_ReturnsMargins()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\li200\\ri400{\\f0\\fs24\\cf1 margin}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Single(data.Segments);
        Assert.Equal(10, data.Segments[0].MarginLeft);
        Assert.Equal(20, data.Segments[0].MarginRight);
        Assert.False(data.Segments[0].Bold);
        Assert.False(data.Segments[0].Italic);
        Assert.False(data.Segments[0].Underline);
    }

    [Fact]
    public void Convert_ParsesStylesheetAndUsesStyleIds()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\b\\cf1 style1;}{\\s2\\i\\cf2 style2;}}{\\s1\\f0\\fs24\\cf1 text1}{\\s2\\f0\\fs24\\cf2 text2}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{ALIGN:left}}text1{{/STYLE}}{{STYLE:2}}text2{{/STYLE}}", data.Markdown);
        Assert.Equal(1, data.Segments[0].StyleId);
        Assert.Equal(2, data.Segments[1].StyleId);
        Assert.True(data.Styles.ContainsKey("1"));
        Assert.True(data.Styles["1"].Bold);
        Assert.True(data.Styles.ContainsKey("2"));
        Assert.True(data.Styles["2"].Italic);
    }

    [Fact]
    public void Convert_DoesNotEmitFontTagsWhenStyleDefinesThem()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\f0\\fs24\\cf2 style1;}}{\\s1\\f0\\fs24\\cf2 text}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{ALIGN:left}}text{{/STYLE}}", data.Markdown);
        Assert.Equal(1, data.Segments[0].StyleId);
        Assert.True(data.Styles.ContainsKey("1"));
        Assert.Equal("Arial", data.Styles["1"].Font);
        Assert.Equal(12, data.Styles["1"].FontSize);
        Assert.Equal("#FF0000", data.Styles["1"].Color.ToHex());
    }
}

