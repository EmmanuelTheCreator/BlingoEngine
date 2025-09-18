using System;
using System.Linq;

using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Sounds;

public class BlLegacySoundReaderTests
{
    [Fact]
    public void ReadDirFileWithThreeSounds_LoadsMp3Payloads()
    {
        var sounds = TestContextHarness.LoadSounds("Sounds/DirFileWith_3_Sounds.dir");

        sounds.Should().HaveCount(3);
        sounds.Select(sound => sound.ResourceId)
            .Should()
            .BeEquivalentTo(new[] { 13, 19, 25 });
        sounds.Select(sound => sound.Bytes.Length)
            .Should()
            .BeEquivalentTo(new[] { 56350, 53474, 6554 });
        sounds.Select(sound => sound.Format)
            .Should()
            .AllBeEquivalentTo(BlLegacySoundFormatKind.Mp3);

        ReadOnlySpan<byte> id3 = stackalloc byte[] { (byte)'I', (byte)'D', (byte)'3' };

        foreach (var sound in sounds)
        {
            sound.Bytes.Length.Should().BeGreaterThan(2);
            var span = sound.Bytes.AsSpan();
            var hasId3 = span.Length >= id3.Length && span[..id3.Length].SequenceEqual(id3);
            var hasFrameSync = span[0] == 0xFF && (span[1] & 0xE0) == 0xE0;
            (hasId3 || hasFrameSync).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("Sounds/DirFileWith_3_Sounds_D10.dcr")]
    [InlineData("Sounds/DirFileWith_3_Sounds_D85.dcr")]
    [InlineData("Sounds/DirFileWith_3_Sounds_compressed_D10.dcr")]
    public void ReadProjectorVariants_LoadsSoundPayloads(string relativePath)
    {
        var sounds = TestContextHarness.LoadSounds(relativePath);

        sounds.Should().HaveCount(3);
        sounds.Select(sound => sound.Format)
            .Should()
            .AllBeEquivalentTo(BlLegacySoundFormatKind.Mp3);
        sounds.Select(sound => sound.Bytes.Length)
            .Should()
            .OnlyContain(length => length > 0);
    }
}
