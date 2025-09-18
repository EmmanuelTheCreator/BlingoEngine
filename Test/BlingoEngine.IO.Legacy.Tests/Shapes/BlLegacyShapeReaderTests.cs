using System;
using System.IO;
using System.Linq;

using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Shapes;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tools;

using FluentAssertions;

using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Shapes;

public class BlLegacyShapeReaderTests
{
    private static readonly byte[] SampleShapeRecord =
    {
        0x00, 0x02, // QuickDraw shape type (oval)
        0xFF, 0xF6, // Top coordinate (-10)
        0xFF, 0xF0, // Left coordinate (-16)
        0x00, 0x14, // Bottom coordinate (20)
        0x00, 0x20, // Right coordinate (32)
        0x12, 0x34, // Fill pattern identifier
        0x80,       // Foreground colour (signed/unsigned boundary)
        0x7F,       // Background colour
        0x1C,       // Fill and ink flags
        0x08,       // Pen thickness
        0x02        // Pattern direction
    };

    [Fact]
    public void ReadDirFileWithShapes_LoadsShapeRecords()
    {
        var shapes = TestContextHarness.LoadShapes("Shapes/DirWith_8_Shapes.dir");

        shapes.Should().HaveCount(8);
        shapes.Select(shape => shape.ResourceId)
            .Should()
            .Equal(4, 5, 6, 7, 8, 9, 10, 11);
        shapes.Select(shape => shape.Bytes.Length)
            .Should()
            .OnlyContain(length => length == 17);
        shapes.Should()
            .OnlyContain(shape => shape.Format == BlLegacyShapeFormatKind.Director4To10UnsignedColors);
    }

    [Fact]
    public void ReadVintageEntry_PreservesSignedPayload()
    {
        var payload = BlLegacyShapeWriter.BuildVintageEntry(SampleShapeRecord);
        var chunk = BuildChunk(payload);

        using var context = CreateContext(chunk, block =>
        {
            block.Format.IsBigEndian = true;
            block.Format.ArchiveVersion = 0x00000100; // Unmapped -> Director 2/3 signed colours
        });

        var shapes = context.ReadShapes();

        shapes.Should().ContainSingle();
        var shape = shapes[0];
        shape.ResourceId.Should().Be(1);
        shape.Format.Should().Be(BlLegacyShapeFormatKind.Director2To3SignedColors);
        shape.Bytes.Should().Equal(SampleShapeRecord);
    }

    [Fact]
    public void ReadDirector4Entry_UsesUnsignedColourInterpretation()
    {
        var infoBytes = new byte[] { 0xAA, 0xBB, 0xCC };
        var payload = BlLegacyShapeWriter.BuildTransitionalEntry(SampleShapeRecord, includeFlags: true, flags: 0x10, infoBytes: infoBytes);
        var chunk = BuildChunk(payload);

        using var context = CreateContext(chunk, block =>
        {
            block.Format.IsBigEndian = true;
            block.Format.ArchiveVersion = 0x00000000; // Director 4 archive marker
        });

        var shapes = context.ReadShapes();

        shapes.Should().ContainSingle();
        var shape = shapes[0];
        shape.Format.Should().Be(BlLegacyShapeFormatKind.Director4To10UnsignedColors);
        shape.Bytes.Should().Equal(SampleShapeRecord);
    }

    [Fact]
    public void ReadModernEntry_ResolvesQuickDrawRecord()
    {
        var infoBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var payload = BlLegacyShapeWriter.BuildModernEntry(SampleShapeRecord, infoBytes: infoBytes);
        var chunk = BuildChunk(payload);

        using var context = CreateContext(chunk, block =>
        {
            block.Format.IsBigEndian = true;
            block.Format.ArchiveVersion = 0x000004C7; // Director 6 marker (modern header)
        });

        var shapes = context.ReadShapes();

        shapes.Should().ContainSingle();
        var shape = shapes[0];
        shape.Format.Should().Be(BlLegacyShapeFormatKind.Director4To10UnsignedColors);
        shape.Bytes.Should().Equal(SampleShapeRecord);
    }

    private static byte[] BuildChunk(byte[] payload)
    {
        using var stream = new MemoryStream();
        var writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
        writer.WriteTag(BlTag.Cast);
        writer.WriteUInt32((uint)payload.Length);
        writer.WriteBytes(payload);
        writer.Flush();
        return stream.ToArray();
    }

    private static ReaderContext CreateContext(byte[] chunk, Action<BlDataBlock> configureBlock, int resourceId = 1)
    {
        var stream = new MemoryStream(chunk, writable: false);
        var context = new ReaderContext(stream, "synthetic", leaveOpen: false);
        var block = new BlDataBlock();
        block.Format.IsBigEndian = true;
        configureBlock(block);
        context.RegisterDataBlock(block);
        context.Reader.Endianness = BlEndianness.BigEndian;
        context.RegisterRifxOffset(0);
        var payloadLength = (uint)(chunk.Length - 8);
        context.AddResource(new BlLegacyResourceEntry(resourceId, BlTag.Cast, payloadLength, 0, 0, 0, 0));
        return context;
    }
}
