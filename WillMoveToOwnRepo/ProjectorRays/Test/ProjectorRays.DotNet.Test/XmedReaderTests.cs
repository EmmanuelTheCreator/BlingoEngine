using System;
using LingoEngine.Director.LGodot.Importer.TestData;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using Xunit;

namespace ProjectorRays.Test;

public class XmedReaderTests
{
    [Fact]
    public void ParsesHalloTextAndStyles()
    {
        var view = CreateView(XmedTestData.HalloDefault);
        var doc = new XmedReader().Read(view);
        Assert.StartsWith("Hallo", doc.Text);
        Assert.Equal("Hallo", doc.Runs[0].Text);
        Assert.Contains(doc.Styles, s => s.FontName == "Arial" && s.ColorIndex == 5);
        Assert.Contains(doc.Styles, s => s.FontName == "Arcade *" && s.ColorIndex == 8);
    }

    [Fact]
    public void ParsesMultiStyleSingleLineStyles()
    {
        var view = CreateView(XmedTestData.MultiStyleSingleLine);
        var doc = new XmedReader().Read(view);
        Assert.True(doc.Styles.Count > 1);
        Assert.Contains(doc.Styles, s => s.FontName == "Arcade *");
        Assert.Contains(doc.Styles, s => s.FontName == "arial");
        Assert.Contains(doc.Styles, s => s.FontName == "Tahoma");
        Assert.Contains(doc.Styles, s => s.FontName == "Terminal");
    }

    [Fact]
    public void ParsesWiderWidth4Text()
    {
        var view = CreateView(XmedTestData.WiderWidth4);
        var doc = new XmedReader().Read(view);
        Assert.Equal("HalloHallo", doc.Text);
        Assert.Equal("Hallo", doc.Runs[0].Text);
        Assert.Equal((uint)203, doc.Width);
        Assert.Contains(doc.Styles, s => s.FontName == "Arial");
        Assert.Contains(doc.Styles, s => s.FontName == "Arcade *");
    }

    private static BufferView CreateView(byte[] data)
    {
        // Test data may contain a preamble, so locate the DEMX header first.
        var pattern = new byte[] { (byte)'D', (byte)'E', (byte)'M', (byte)'X' };
        var start = Array.IndexOf(data, pattern[0]);
        while (start >= 0 && start + 3 < data.Length)
        {
            if (data[start + 1] == pattern[1] && data[start + 2] == pattern[2] && data[start + 3] == pattern[3])
                break;
            start = Array.IndexOf(data, pattern[0], start + 1);
        }
        Assert.True(start >= 0, "XMED header not found");
        return new BufferView(data, start, data.Length - start);
    }
}
