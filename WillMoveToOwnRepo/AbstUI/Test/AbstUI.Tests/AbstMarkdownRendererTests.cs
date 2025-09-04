using System.Collections.Generic;
using AbstUI.Components.Graphics;
using AbstUI.Styles;
using AbstUI.Texts;
using AbstUI.Primitives;

namespace AbstUI.Tests;

public class AbstMarkdownRendererTests
{
    private class TestFontManager : IAbstFontManager
    {
        private readonly int _topIndent;
        private readonly Dictionary<string, int> _topIndents;

        public TestFontManager(int topIndent = 0, Dictionary<string, int>? topIndents = null)
        {
            _topIndent = topIndent;
            _topIndents = topIndents ?? new();
        }

        public IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular) => this;
        public void LoadAll() { }
        public T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class => null;
        public T GetDefaultFont<T>() where T : class => null!;
        public void SetDefaultFont<T>(T font) where T : class { }
        public IEnumerable<string> GetAllNames() => System.Array.Empty<string>();
        public float MeasureTextWidth(string text, string fontName, int fontSize) => text.Length * fontSize;
        public FontInfo GetFontInfo(string fontName, int fontSize)
        {
            int indent = _topIndents.TryGetValue(fontName, out var ti) ? ti : _topIndent;
            return new(fontSize, indent);
        }
    }

    private class RecordingPainter : IAbstImagePainter
    {
        public List<APoint> TextPositions { get; } = new();
        public List<int> FontSizes { get; } = new();

        public int Height { get; set; }
        public int Width { get; set; }
        public bool Pixilated { get; set; }
        public bool AutoResize { get; set; }
        public string Name { get; set; } = string.Empty;

        public void Clear(AColor color) { }
        public void SetPixel(APoint point, AColor color) { }
        public void DrawLine(APoint start, APoint end, AColor color, float width = 1) { }
        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1) { }
        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1) { }
        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1) { }
        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1) { }
        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
        {
            TextPositions.Add(position);
            FontSizes.Add(fontSize);
        }
        public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
        {
            TextPositions.Add(position);
            FontSizes.Add(fontSize);
        }
        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format) { }
        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position) { }
        public IAbstTexture2D GetTexture(string? name = null) => null!;
        public void Render() { }
        public void Dispose() { }
    }

    private static AbstMarkdownRenderer CreateRenderer(int topIndent = 0, Dictionary<string, int>? topIndents = null)
        => new(new TestFontManager(topIndent, topIndents));

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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + lineHeight - 4, painter.TextPositions[1].Y);
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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
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
        Assert.Equal(start.Y - 4, painter.TextPositions[0].Y);
        Assert.Equal(start.Y + style.LineHeight - 4, painter.TextPositions[1].Y);
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
        Assert.Equal(16, painter.TextPositions[0].Y);
        Assert.Equal(10, painter.TextPositions[1].Y);
        Assert.Equal(16, painter.TextPositions[2].Y);
    }
}
