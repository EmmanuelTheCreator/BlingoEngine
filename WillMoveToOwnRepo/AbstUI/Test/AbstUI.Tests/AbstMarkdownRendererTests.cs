using System.Collections.Generic;
using AbstUI.Texts;
using AbstUI.Primitives;
using AbstUI.Tests.Common;

namespace AbstUI.Tests;
public partial class AbstMarkdownRendererTests
{


    private static AbstMarkdownRenderer CreateRenderer(int topIndent = 0, Dictionary<string, int>? topIndents = null, Dictionary<string, int>? extraHeights = null)
        => new(new TestFontManager(topIndent, topIndents, extraHeights));


    private static AbstTextStyle CreateDefaultStyle()
        => new() { Name = "default", Font = "Arial", FontSize = 12, Color = AColors.Black };

    [Fact]
    public void DoFastRendering_True_ForPlainTextWithSingleStyle()
    {
        var renderer = CreateRenderer();
        renderer.SetText("Hello world", new[] { CreateDefaultStyle() });
        Assert.True(renderer.DoFastRendering);
    }

    [Fact]
    public void DoFastRendering_False_WhenSpecialTagsPresent()
    {
        var renderer = CreateRenderer();
        renderer.SetText("# Heading", new[] { CreateDefaultStyle() });
        Assert.False(renderer.DoFastRendering);
    }

    [Fact]
    public void DoFastRendering_False_WithMultipleStyles()
    {
        var renderer = CreateRenderer();
        var styles = new[]
        {
            CreateDefaultStyle(),
            new AbstTextStyle { Name = "other", Font = "Arial", FontSize = 12 }
        };
        renderer.SetText("Hello world", styles);
        Assert.False(renderer.DoFastRendering);
    }

    [Fact]
    public void FastRender_SingleLine_UsesTopIndentation()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        renderer.SetText("Hello world", new[] { style });
        Assert.True(renderer.DoFastRendering);

        var painter = new RecordingPainter();

        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Single(painter.TextPositions);
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
    }

    [Fact]
    public void FastRender_MultiLine_ComputesLinePositions()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        renderer.SetText("Line1\nLine2\nLine3", new[] { style });
        Assert.True(renderer.DoFastRendering);

        var painter = new RecordingPainter();

        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Equal(3, painter.TextPositions.Count);
        int lineHeight = style.FontSize;
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + lineHeight, painter.TextPositions[1].Y);
    }

    [Fact]
    public void SlowRender_SingleLine_UsesTopIndentation()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        renderer.SetText("Hello#", new[] { style });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();

        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Single(painter.TextPositions);
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
    }

    [Fact]
    public void SlowRender_MultiLine_ComputesLinePositions()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        renderer.SetText("Line1#\nLine2\nLine3", new[] { style });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();

        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Equal(3, painter.TextPositions.Count);
        int lineHeight = style.FontSize;
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + lineHeight, painter.TextPositions[1].Y);
    }

    [Fact]
    public void FastRender_UsesExplicitLineHeight()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        style.LineHeight = 20;
        renderer.SetText("Line1\nLine2", new[] { style });
        Assert.True(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Equal(2, painter.TextPositions.Count);
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + style.LineHeight, painter.TextPositions[1].Y);
    }

    [Fact]
    public void SlowRender_UsesExplicitLineHeight()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        style.LineHeight = 20;
        renderer.SetText("Line1#\nLine2", new[] { style });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        var start = new APoint(0, 20);
        renderer.Render(painter, start);

        Assert.Equal(2, painter.TextPositions.Count);
        Assert.Equal(start.Y, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + style.LineHeight, painter.TextPositions[1].Y);
    }

    [Fact]
    public void SlowRender_HandlesInlineFontSizeChanges()
    {
        var renderer = CreateRenderer();
        renderer.SetText("{{FONT-SIZE:14}}Enter your {{FONT-SIZE:18}}Name", new[] { CreateDefaultStyle() });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        renderer.Render(painter, new APoint(0, 0));

        Assert.Equal(2, painter.TextPositions.Count);
        Assert.Equal(0, painter.TextPositions[0].X);
        Assert.Equal(14, painter.FontSizes[0]);
        // width of "Enter your " (11 characters) at size 14 -> 154
        Assert.Equal(154, painter.TextPositions[1].X);
        Assert.Equal(18, painter.FontSizes[1]);
    }

    [Fact]
    public void SlowRender_AlignsSegments_WithDifferentTopIndentations()
    {
        var topIndents = new Dictionary<string, int>
        {
            ["A"] = 4,
            ["B"] = 10
        };
        var renderer = CreateRenderer(topIndents: topIndents);
        var style = new AbstTextStyle { Name = "default", Font = "A", FontSize = 12 };
        renderer.SetText("Hello {{FONT-FAMILY:B}}g{{FONT-FAMILY:A}}!", new[] { style });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        renderer.Render(painter, new APoint(0, 20));

        Assert.Equal(3, painter.TextPositions.Count);
        Assert.Equal(26, painter.TextPositions[0].Y);
        Assert.Equal(20, painter.TextPositions[1].Y);
        Assert.Equal(26, painter.TextPositions[2].Y);

    }

    [Fact]
    public void SlowRender_RespectsFontHeight_ForLineAdvance()
    {
        var renderer = CreateRenderer(extraHeights: new() { ["Arial"] = 4 });
        renderer.SetText("Line1#\nLine2", new[] { CreateDefaultStyle() });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        renderer.Render(painter, new APoint(0, 0));

        Assert.Equal(2, painter.TextPositions.Count);
        Assert.Equal(0, painter.TextPositions[0].Y);
        Assert.Equal(16, painter.TextPositions[1].Y);
    }

    [Fact]
    public void SlowRender_SkipsEmptyLines()
    {
        var renderer = CreateRenderer(topIndent: 4);
        var style = CreateDefaultStyle();
        renderer.SetText("Line1#\n\nLine3", new[] { style });
        Assert.False(renderer.DoFastRendering);

        var painter = new RecordingPainter();
        renderer.Render(painter, new APoint(0, 20));

        Assert.Equal(2, painter.TextPositions.Count);
        Assert.Equal(20, painter.TextPositions[0].Y);
        Assert.Equal(32, painter.TextPositions[1].Y);

    }
}
