using System;
using System.Linq;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using LingoEngine.Tools;
using FluentAssertions;
using Xunit;
using AbstUI.Tests.Common;

namespace LingoEngine.Tests;

public class AbstMarkdownRendererTests
{
    private const string DefaultMarkdown = "{{PARA:1}}New **Highscore!!!**\n{{PARA}}{{FONT-SIZE:14}}Enter your {{FONT-SIZE:18}}Name";

    private static void ApplyDefaultMarkdown(AbstMarkdownRenderer renderer, AbstTextStyle style)
        => renderer.SetText(DefaultMarkdown, new[] { style });

    private static (AbstMarkdownRenderer renderer, RecordingPainter painter) CreateRenderer(
        Action<AbstMarkdownRenderer, AbstTextStyle>? configure = null)
    {
        var fontManager = new TestFontManager();
        var renderer = new AbstMarkdownRenderer(fontManager);
        var style = new AbstTextStyle { Name = "1", Font = "Arial", FontSize = 12 };
        (configure ?? ApplyDefaultMarkdown)(renderer, style);
        var painter = new RecordingPainter { AutoResizeWidth = true, AutoResizeHeight = true };
        renderer.Render(painter, new APoint(0, 0));
        return (renderer, painter);
    }

    [Fact]
    public void Render_RendersParagraphsOnSeparateLines()
    {
        var (_, painter) = CreateRenderer();
        painter.TextCalls.Should().HaveCount(4);
        painter.TextCalls[0].Position.Y.Should().Be(0);
        painter.TextCalls[1].Position.Y.Should().Be(0);
        painter.TextCalls[2].Position.Y.Should().Be(12);
        painter.TextCalls[3].Position.Y.Should().Be(12);
    }

    [Fact]
    public void Render_UsesLargestFontHeightPerLine()
    {
        var (_, painter) = CreateRenderer();
        painter.Height.Should().Be(30);
        var lineHeights = painter.TextCalls
            .GroupBy(c => c.Position.Y)
            .Select(g => g.Max(c => c.Height))
            .OrderBy(h => h)
            .ToArray();
        lineHeights.Should().Equal(new[] { 12, 18 });
    }

    [Fact]
    public void Render_FromRtfConvertedMarkdown_HasExpectedSize()
    {
        const string rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fnil Arcade *;}{\\f2\\fnil Earth *;}}{\\colortbl\\red0\\green0\\blue0;\\red255\\green0\r\n\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}\\pard \\f0\\fs24{\\pard \\f2\\fs36\\cf1\\qc New }{\\pard \\b\\f2\\fs36\\cf1\\qc Highscore!!!}{\\pard \r\n\\f2\\fs36\\cf1\\qc\\par\r\n}{\\pard \r\n\\f2\\fs28\\cf1\\qc Enter your }{\\pard \\f2\\fs36\\cf1\\qc Name}}";

        var data = RtfToMarkdown.Convert(rtf);

        var (_, painter) = CreateRenderer((renderer, _) => renderer.SetText(data));

        var expected = "{{PARA:1}}New **Highscore!!!**\n{{PARA:2}}Enter your {{FONT-SIZE:18}}Name";
        data.Markdown.Should().Be(expected);
        painter.Height.Should().BeGreaterThanOrEqualTo(18);
    }

    [Fact]
    public void Render_ParsesEmbeddedStyleSheet()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs40\\cf1 big}}";
        var data = RtfToMarkdown.Convert(rtf, includeStyleSheet: true);
        var fontManager = new TestFontManager();
        var renderer = new AbstMarkdownRenderer(fontManager);
        var painter = new RecordingPainter { AutoResizeWidth = true, AutoResizeHeight = true };
        renderer.SetText(data.Markdown);
        renderer.Render(painter, new APoint(0, 0));
        painter.FontSizes[0].Should().Be(20);
    }
}
