using System.Collections.Generic;
using System.IO;
using System.Linq;

using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Files;
using BlingoEngine.IO.Legacy.Tools;

using FluentAssertions;

using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Data;

public class BlDataBlockExtractionTests
{
    [Theory]
    [MemberData(nameof(ClassicDirectorCases))]
    public void ReadDirFilesContainer_ExtractsResourceTagsAcrossDirectorVersions(ClassicDirectorCase scenario)
    {
        var movieBytes = BuildClassicMovieBytes(scenario);
        using var stream = new MemoryStream(movieBytes, writable: false);
        using var context = new ReaderContext(stream, $"{scenario.Name}.dir", leaveOpen: true);

        var container = context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var block = context.DataBlock!;

        block.PayloadStart.Should().Be(12);
        block.DeclaredSize.Should().Be((uint)(movieBytes.Length - 12));
        block.PayloadEnd.Should().Be(block.PayloadStart + block.DeclaredSize);

        var format = block.Format;
        format.Codec.Should().Be(scenario.ExpectedCodec);
        format.ArchiveVersion.Should().Be(scenario.ArchiveVersion);
        format.MapVersion.Should().Be(scenario.MapVersion);
        format.DirectorVersion.Should().Be(scenario.ExpectedDirectorVersion);
        format.DirectorVersionLabel.Should().Be($"Director {scenario.ExpectedDirectorVersion}");

        var resourceTags = context.Resources.Entries.Select(entry => entry.Tag.Value).ToList();
        resourceTags.Should().BeEquivalentTo(scenario.ResourceTags);

        container.Files.Should().BeEmpty();
    }

    public static IEnumerable<object[]> ClassicDirectorCases()
    {
        yield return new object[]
        {
            new ClassicDirectorCase(
                Name: "director4-little",
                Magic: "XFIR",
                CodecTag: "MV93",
                ArchiveVersion: 0x00000000,
                MapVersion: 0,
                ExpectedDirectorVersion: 4,
                ExpectedCodec: BlRifxCodec.MV93,
                ResourceTags: new[] { "CASt", "Lscr" })
        };

        yield return new object[]
        {
            new ClassicDirectorCase(
                Name: "director5-big",
                Magic: "RIFX",
                CodecTag: "MC95",
                ArchiveVersion: 0x000004C1,
                MapVersion: 1,
                ExpectedDirectorVersion: 5,
                ExpectedCodec: BlRifxCodec.MC95,
                ResourceTags: new[] { "CASt", "Lscr", "KEY*" })
        };

        yield return new object[]
        {
            new ClassicDirectorCase(
                Name: "director6-little",
                Magic: "XFIR",
                CodecTag: "MC95",
                ArchiveVersion: 0x000004C7,
                MapVersion: 1,
                ExpectedDirectorVersion: 6,
                ExpectedCodec: BlRifxCodec.MC95,
                ResourceTags: new[] { "CASt" })
        };

        yield return new object[]
        {
            new ClassicDirectorCase(
                Name: "director85-big",
                Magic: "RIFX",
                CodecTag: "MV93",
                ArchiveVersion: 0x00000708,
                MapVersion: 1,
                ExpectedDirectorVersion: 8,
                ExpectedCodec: BlRifxCodec.MV93,
                ResourceTags: new[] { "CASt", "Lscr", "VWFI" })
        };

        yield return new object[]
        {
            new ClassicDirectorCase(
                Name: "director10-little",
                Magic: "XFIR",
                CodecTag: "APPL",
                ArchiveVersion: 0x00000742,
                MapVersion: 1,
                ExpectedDirectorVersion: 10,
                ExpectedCodec: BlRifxCodec.APPL,
                ResourceTags: new[] { "CASt", "Lscr", "STXT" })
        };
    }

    private static byte[] BuildClassicMovieBytes(ClassicDirectorCase scenario)
    {
        var littleEndian = scenario.Magic is "XFIR" or "RIFF";
        using var stream = new MemoryStream();
        var endianness = littleEndian ? BlEndianness.LittleEndian : BlEndianness.BigEndian;
        var writer = new BlStreamWriter(stream)
        {
            Endianness = endianness
        };

        writer.WriteAscii(scenario.Magic);
        var sizeOffset = writer.Position;
        writer.WriteUInt32(0); // placeholder
        writer.WriteTag(scenario.CodecTag);

        var payloadStart = writer.Position;

        writer.WriteTag("imap");
        writer.WriteUInt32(16);
        writer.WriteUInt32(16);
        writer.WriteUInt32(scenario.MapVersion);
        var mapOffsetPosition = writer.Position;
        writer.WriteUInt32(0); // patched later
        writer.WriteUInt32(scenario.ArchiveVersion);

        var mmapChunkStart = writer.Position;
        writer.WriteTag("mmap");

        const ushort headerSize = 12;
        const ushort entrySize = 20;
        var entryCount = scenario.ResourceTags.Count;
        var mmapPayloadLength = headerSize + entryCount * entrySize;

        writer.WriteUInt32((uint)mmapPayloadLength);
        writer.WriteUInt16(headerSize);
        writer.WriteUInt16(entrySize);
        writer.WriteUInt32((uint)entryCount);
        writer.WriteUInt32((uint)entryCount);

        foreach (var tag in scenario.ResourceTags)
        {
            writer.WriteTag(tag);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            writer.WriteUInt32(0);
        }

        var mapOffset = (uint)(mmapChunkStart - payloadStart);
        var payloadLength = (uint)(writer.Position - payloadStart);

        writer.Position = mapOffsetPosition;
        writer.WriteUInt32(mapOffset);

        writer.Position = sizeOffset;
        writer.WriteUInt32(payloadLength);

        return stream.ToArray();
    }

    public sealed record ClassicDirectorCase(
        string Name,
        string Magic,
        string CodecTag,
        uint ArchiveVersion,
        uint MapVersion,
        int ExpectedDirectorVersion,
        BlRifxCodec ExpectedCodec,
        IReadOnlyList<string> ResourceTags);
}
