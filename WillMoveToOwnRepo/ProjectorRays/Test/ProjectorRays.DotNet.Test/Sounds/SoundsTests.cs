using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Sounds;

public class SoundsTests
{
    private readonly ILogger<DirectorFileTests> _logger;

    public SoundsTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<DirectorFileTests>();
    }

    [Fact]
    public void DirFileWithThreeSounds_ExtractsSoundChunks()
    {
        var path = GetPath("Sounds/DirFileWith_3_Sounds.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);

        dir.Read(stream).Should().BeTrue("because the director file should load successfully");

        var cast = dir.Casts.Should().ContainSingle().Subject;
        var soundMembers = cast.Members.Values
            .Where(m => m.Type == RaysMemberType.SoundMember)
            .OrderBy(m => m.Id)
            .ToList();

        soundMembers.Should().HaveCount(3, "because the DirFileWith_3_Sounds fixture contains three sound members");
        soundMembers
            .Select(m => m.Info?.Name ?? string.Empty)
            .Should()
            .Equal("level_up", "go", "blockfall_1");

        var expectedSizes = new[] { 56350, 53474, 6554 };

        for (int i = 0; i < soundMembers.Count; i++)
        {
            var soundData = soundMembers[i].SoundData;
            soundData.Size.Should().BeGreaterThan(0,
                "because sound member {0} should not be empty",
                soundMembers[i].Id);
            StartsWithMp3Header(soundData).Should().BeTrue(
                "because sound member {0} should start with an MP3 header",
                soundMembers[i].Id);

            var chunkData = soundData.ToArray();
            chunkData.Length.Should().Be(expectedSizes[i],
                "because sound member {0} should have the expected byte count",
                soundMembers[i].Id);

            var hasMp3Header = StartsWithId3(chunkData) || StartsWithMp3Frame(chunkData);
            hasMp3Header.Should().BeTrue(
                "because sound member {0} should start with an MP3 header",
                soundMembers[i].Id);
        }
    }

    private static bool StartsWithMp3Header(BufferView data)
    {
        var bytes = data.Data;
        int offset = data.Offset;
        if (data.Size < 3)
            return false;
        return (bytes[offset] == 0x49 && bytes[offset + 1] == 0x44 && bytes[offset + 2] == 0x33) ||
               (bytes[offset] == 0xFF && (bytes[offset + 1] & 0xE0) == 0xE0);
    }

    private static bool StartsWithId3(byte[] bytes)
        => bytes.Length >= 3 && bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33;

    private static bool StartsWithMp3Frame(byte[] bytes)
        => bytes.Length >= 2 && bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0;

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
