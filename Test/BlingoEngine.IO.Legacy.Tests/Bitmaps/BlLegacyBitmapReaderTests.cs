using System.IO;

using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Bitmaps;

public class BlLegacyBitmapReaderTests
{
    [Fact]
    public void ReadImgCastCst_LoadsPngAndAuxiliaryChunks()
    {
        var bitmaps = TestContextHarness.LoadBitmaps("Images/ImgCast.cst");

        bitmaps.Should().NotBeEmpty();

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 13
            && bitmap.Format == BlLegacyBitmapFormatKind.Png
            && bitmap.Bytes.Length == 529);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 14
            && bitmap.Format == BlLegacyBitmapFormatKind.Bitd
            && bitmap.Bytes.Length == 1001);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 15
            && bitmap.Format == BlLegacyBitmapFormatKind.AlphaMask
            && bitmap.Bytes.Length == 125);

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 16
            && bitmap.Format == BlLegacyBitmapFormatKind.Thumbnail
            && bitmap.Bytes.Length == 1303);
    }

    [Fact]
    public void ReadDirFileWithBitmap_FallsBackToBitdWhenEditorUnknown()
    {
        var bitmaps = TestContextHarness.LoadBitmaps("Images/Dir_With_One_Img_Sprite_Hallo.dir");

        bitmaps.Should().Contain(bitmap =>
            bitmap.ResourceId == 32
            && bitmap.Format == BlLegacyBitmapFormatKind.Bitd
            && bitmap.Bytes.Length == 1001);
    }

    [Fact]
    public void ReadDirector3BitmapFromKeyTable_LoadsBitdPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyBitmapWriter(stream);
        var bitdPayload = new byte[] { 0x01, 0x00, 0x81, 0x7F, 0x02 };
        var bitdEntry = writer.WriteBitd(101, bitdPayload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(bitdEntry);
        context.AddResourceRelationship(new BlResourceKeyLink(bitdEntry.Id, 1, bitdEntry.Tag));

        var bitmaps = context.ReadBitmaps();

        bitmaps.Should().HaveCount(1);
        var bitmap = bitmaps[0];
        bitmap.ResourceId.Should().Be(bitdEntry.Id);
        bitmap.Format.Should().Be(BlLegacyBitmapFormatKind.Bitd);
        bitmap.Bytes.Should().Equal(bitdPayload);
    }

    [Fact]
    public void ReadDirector4BitmapWithDibChild_LoadsWindowsPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyBitmapWriter(stream);
        var dibPayload = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x08, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00,
            0x13, 0x0B, 0x00, 0x00,
            0x13, 0x0B, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        };
        var dibEntry = writer.WriteDib(102, dibPayload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(dibEntry);
        context.AddResourceRelationship(new BlResourceKeyLink(dibEntry.Id, 2, dibEntry.Tag));

        var bitmaps = context.ReadBitmaps();

        bitmaps.Should().HaveCount(1);
        var bitmap = bitmaps[0];
        bitmap.ResourceId.Should().Be(dibEntry.Id);
        bitmap.Format.Should().Be(BlLegacyBitmapFormatKind.Dib);
        bitmap.Bytes.Should().Equal(dibPayload);
    }

    [Fact]
    public void ReadDirector4BitmapWithBitdAndDib_ReturnsBothChunks()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyBitmapWriter(stream);
        var bitdPayload = new byte[] { 0x00, 0x02, 0xAA, 0x82, 0x03 };
        var bitdEntry = writer.WriteBitd(120, bitdPayload);

        var dibPayload = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x08, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x20, 0x00, 0x00, 0x00,
            0x13, 0x0B, 0x00, 0x00,
            0x13, 0x0B, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        };
        var dibEntry = writer.WriteDib(121, dibPayload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(bitdEntry);
        context.AddResource(dibEntry);
        context.AddResourceRelationship(new BlResourceKeyLink(bitdEntry.Id, 3, bitdEntry.Tag));
        context.AddResourceRelationship(new BlResourceKeyLink(dibEntry.Id, 3, dibEntry.Tag));

        var bitmaps = context.ReadBitmaps();

        bitmaps.Should().HaveCount(2);
        bitmaps[0].ResourceId.Should().Be(bitdEntry.Id);
        bitmaps[0].Format.Should().Be(BlLegacyBitmapFormatKind.Bitd);
        bitmaps[0].Bytes.Should().Equal(bitdPayload);

        bitmaps[1].ResourceId.Should().Be(dibEntry.Id);
        bitmaps[1].Format.Should().Be(BlLegacyBitmapFormatKind.Dib);
        bitmaps[1].Bytes.Should().Equal(dibPayload);
    }
}
