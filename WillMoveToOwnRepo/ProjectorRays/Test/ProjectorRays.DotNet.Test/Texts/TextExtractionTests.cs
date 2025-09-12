using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Texts;

public class TextExtractionTests
{
    private readonly ILogger<TextExtractionTests> _logger;

    public TextExtractionTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<TextExtractionTests>();
    }

    [Fact]
    public void TextCastContainsTextMember()
    {
        var path = GetPath("Texts_Fields/Text_Hallo_fontsize14.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        var cast = Assert.Single(dir.Casts);
        var member = Assert.Single(cast.Members.Values, m =>
            m.Type == RaysMemberType.TextMember || m.Type == RaysMemberType.FieldMember);

        var xmedInfo = dir.GetLastXMED();
        Assert.NotNull(xmedInfo);
        var xmedChunk = (RaysXmedChunk)dir.GetChunk(xmedInfo!.FourCC, xmedInfo.Id);
        var decoded = RaysCastMemberTextRead.FromXmedChunk(xmedChunk.Data, dir);

        Assert.Contains("Hallo", decoded.Text);
        Assert.Contains(decoded.Styles, s => s.FontSize == 14);
    }

    [Fact]
    public void TextMemberExposesName()
    {
        var path = GetPath("Texts_Fields/Text_Hallo_with_name.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        var cast = Assert.Single(dir.Casts);
        var member = Assert.Single(cast.Members.Values, m =>
            m.Type == RaysMemberType.TextMember || m.Type == RaysMemberType.FieldMember);

        Assert.Equal("MyText", member.GetName());
    }

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
