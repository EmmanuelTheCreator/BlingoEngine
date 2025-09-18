using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
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

    [Fact]
    public void ReadStandaloneMacSoundEntry_LoadsRawBytes()
    {
        using var stream = new MemoryStream();
        var payload = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        WriteChunk(stream, "SND ", payload);
        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        var entry = new BlLegacyResourceEntry(1, BlTag.Register("SND "), (uint)payload.Length, 0, 0, 0, 0);
        context.AddResource(entry);

        var sounds = context.ReadSounds();

        sounds.Should().HaveCount(1);
        var sound = sounds[0];
        sound.ResourceId.Should().Be(1);
        sound.Bytes.Should().Equal(payload);
        sound.Format.Should().Be(BlLegacySoundFormatKind.Unknown);
    }

    [Fact]
    public void ReadSoundFromKeyTable_PrefersSampleWhenEditorMissing()
    {
        using var stream = new MemoryStream();
        var sample = Encoding.ASCII.GetBytes("PCM");
        WriteChunk(stream, "sndS", sample);
        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());

        var sampleEntry = new BlLegacyResourceEntry(2, BlTag.Register("sndS"), (uint)sample.Length, 0, 0, 0, 0);
        context.AddResource(sampleEntry);

        context.AddResourceRelationship(new BlResourceKeyLink(sampleEntry.Id, 100, BlTag.Register("sndS")));

        var sounds = context.ReadSounds();

        sounds.Should().HaveCount(1);
        var sound = sounds[0];
        sound.ResourceId.Should().Be(sampleEntry.Id);
        sound.Bytes.Should().Equal(sample);
    }

    private static void WriteChunk(Stream stream, string tag, byte[] payload)
    {
        var tagBytes = Encoding.ASCII.GetBytes(tag);
        if (tagBytes.Length != 4)
        {
            throw new ArgumentException("Tag must contain exactly four characters.", nameof(tag));
        }

        stream.Write(tagBytes, 0, tagBytes.Length);
        Span<byte> lengthBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(lengthBuffer, (uint)payload.Length);
        stream.Write(lengthBuffer);
        stream.Write(payload, 0, payload.Length);
    }
}
