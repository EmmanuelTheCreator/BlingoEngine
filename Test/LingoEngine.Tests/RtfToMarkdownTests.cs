using System.Linq;
using AbstUI.Texts;
using LingoEngine.Texts;
using LingoEngine.Tools;
using Xunit;

public class RtfToMarkdownTests
{
    [Fact]
    public void Convert_ReturnsStyledMarkdown_SingleLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 plain}{\\b\\f0\\fs24\\cf1 bold}{\\i\\f0\\fs24\\cf1 italic}{\\ul\\f0\\fs24\\cf1 underline}}";

        var (markdown, segments, _) = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:left}} plain** bold*** italic*__ underline__", markdown);
        Assert.Equal(new[]
        {
            LingoTextStyle.None,
            LingoTextStyle.Bold,
            LingoTextStyle.Italic,
            LingoTextStyle.Underline
        }, segments.Select(s => s.Style));
    }

    [Fact]
    public void Convert_ReturnsStyledMarkdown_MultiLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue255;}{\\f0\\fs24\\cf1 line1\\par}{\\i\\f0\\fs24\\cf2 line2}}";

        var (markdown, segments, _) = RtfToMarkdown.Convert(rtf);

        var expected = "{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:left}} line1\n{{COLOR:#0000FF}}* line2*";
        Assert.Equal(expected, markdown);
        Assert.Equal(new[]
        {
            LingoTextStyle.None,
            LingoTextStyle.Italic
        }, segments.Select(s => s.Style));
    }

    [Fact]
    public void Convert_ParsesRightAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qr{\\f0\\fs24\\cf1 right}}";

        var (markdown, segments, _) = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:right}} right", markdown);
        Assert.Single(segments);
        Assert.Equal(AbstTextAlignment.Right, segments[0].Alignment);
    }

    [Fact]
    public void Convert_ParsesCenterAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qc{\\f0\\fs24\\cf1 center}}";

        var (markdown, segments, _) = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:center}} center", markdown);
        Assert.Single(segments);
        Assert.Equal(AbstTextAlignment.Center, segments[0].Alignment);
    }

    [Fact]
    public void Convert_ReturnsMargins()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\li200\\ri400{\\f0\\fs24\\cf1 margin}}";

        var (_, segments, _) = RtfToMarkdown.Convert(rtf);

        Assert.Single(segments);
        Assert.Equal(10, segments[0].MarginLeft);
        Assert.Equal(20, segments[0].MarginRight);
        Assert.Equal(LingoTextStyle.None, segments[0].Style);
    }

    [Fact]
    public void Convert_ParsesStylesheetAndUsesStyleIds()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\b\\cf1 style1;}{\\s2\\i\\cf2 style2;}}{\\s1\\f0\\fs24\\cf1 text1}{\\s2\\f0\\fs24\\cf2 text2}}";

        var (markdown, segments, styles) = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{ALIGN:left}} text1{{/STYLE}}{{STYLE:2}} text2{{/STYLE}}", markdown);
        Assert.Equal(1, segments[0].StyleId);
        Assert.Equal(2, segments[1].StyleId);
        Assert.True(styles.ContainsKey("1"));
        Assert.True(styles["1"].Bold);
        Assert.True(styles.ContainsKey("2"));
        Assert.True(styles["2"].Italic);
    }

    [Fact]
    public void Convert_DoesNotEmitFontTagsWhenStyleDefinesThem()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\f0\\fs24\\cf2 style1;}}{\\s1\\f0\\fs24\\cf2 text}}";

        var (markdown, segments, styles) = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{ALIGN:left}} text{{/STYLE}}", markdown);
        Assert.Equal(1, segments[0].StyleId);
        Assert.True(styles.ContainsKey("1"));
        Assert.Equal("Arial", styles["1"].Font);
        Assert.Equal(12, styles["1"].FontSize);
        Assert.Equal("#FF0000", styles["1"].Color.ToHex());
    }
}

