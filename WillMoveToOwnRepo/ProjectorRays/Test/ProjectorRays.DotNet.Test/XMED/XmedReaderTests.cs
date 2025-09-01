using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.Director;
using ProjectorRays.DotNet.Test.TestData;
using ProjectorRays.IO;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.XMED;

public class XmedReaderTests
{

    private readonly ILogger<DirectorFileTests> _logger;

    public XmedReaderTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<DirectorFileTests>();
    }

    [Fact]
    public void TextCastTextContainsHallo()
    {
        var path = GetPath("Texts_Fields/Text_Hallo_fontsize14.cst");
        var data = File.ReadAllBytes(path);
        var view = CreateView(data);
        var doc = new XmedReader().Read(view);
        Assert.Contains("Hallo", doc.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldCastTextContainsHallo()
    {
        var path = GetPath("Texts_Fields/Field_Hallo.cst");
        var data = File.ReadAllBytes(path);
        var view = CreateView(data);
        var doc = new XmedReader().Read(view);
        Assert.Contains("Hallo", doc.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DirWithOneTextSpriteContainsText()
    {
        var path = GetPath("Images/Dir_With_One_Tex_Sprite_Hallo.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));
    }


    [Fact]
    public void RestoresScriptTextIntoMembers()
    {
        var path = GetPath("Texts_Fields/TextCast.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        dir.RestoreScriptText();

        // todo test it
    }

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
    public void LogUnkownBytes()
    {
        var view = CreateView(XmedTestData.MultiStyleMultiLine);
        var doc = new XmedReader().Read(view);
        foreach (var run in doc.Runs)
        {
            _logger.LogInformation("---------");
            _logger.LogInformation("Text=" + run.Text);
            _logger.LogInformation("---------");
            _logger.LogInformation(nameof(run.Unknown1) + "=" + run.Unknown1);
            _logger.LogInformation(nameof(run.Unknown2) + "=" + run.Unknown2);
            _logger.LogInformation(nameof(run.Unknown3) + "=" + run.Unknown3);
            _logger.LogInformation(nameof(run.Unknown4) + "=" + run.Unknown4);
            _logger.LogInformation(nameof(run.Unknown5) + "=" + run.Unknown5);
            _logger.LogInformation(nameof(run.Unknown6) + "=" + run.Unknown6);
            _logger.LogInformation(nameof(run.Unknown7) + "=" + run.Unknown7);
            _logger.LogInformation(nameof(run.Unknown8) + "=" + run.Unknown8);
            _logger.LogInformation(nameof(run.Unknown9) + "=" + run.Unknown9);
            _logger.LogInformation(nameof(run.Unknown10) + "=" + run.Unknown10);
            _logger.LogInformation(nameof(run.Unknown11) + "=" + run.Unknown11);
        }
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

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
