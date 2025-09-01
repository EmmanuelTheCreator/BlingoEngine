using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test;

public class TextFieldCastTests
{
    private readonly ILogger<TextFieldCastTests> _logger;

    public TextFieldCastTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });
        _logger = factory.CreateLogger<TextFieldCastTests>();
    }

    public static IEnumerable<object[]> CastFiles()
    {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "../../../../TestData/Texts_Fields");
        foreach (var file in Directory.EnumerateFiles(baseDir, "*.cst"))
        {
            var relative = Path.Combine("Texts_Fields", Path.GetFileName(file));
            yield return new object[] { relative };
        }
    }

    [Theory]
    [MemberData(nameof(CastFiles))]
    public void CastFileLoadsAndHasExpectedType(string relativePath)
    {
        var path = GetPath(relativePath);
        var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        Assert.NotEmpty(dir.Casts);
        bool found = false;
        foreach (var id in dir.Casts[0].MemberIDs)
        {
            var c = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
            if (c.Type == RaysMemberType.FieldMember || c.Type == RaysMemberType.TextMember)
            {
                found = true;
                break;
            }
        }

        Assert.True(found);
    }

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
