using System.IO;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Files;
using BlingoEngine.IO.Legacy.Sounds;

using FluentAssertions;

using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Files;

public class BlLegacyFileWriterTests
{
    [Fact]
    public void DirWriter_RoundTripsResources()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x01, 0x02, 0x03 }
        });
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "Lscr_0001.bin",
            Bytes = new byte[] { 0x10, 0x11 }
        });

        var writer = new BlDirFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dir", leaveOpen: true);
        var roundTrip = context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var block = context.DataBlock!;
        block.PayloadStart.Should().Be(12);
        block.Format.IsBigEndian.Should().BeFalse();
        block.Format.Codec.Should().Be(BlRifxCodec.MV93);
        block.Format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        block.Format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);

        roundTrip.Files.Should().HaveCount(2);
        roundTrip.Files[0].Bytes.Should().Equal(container.Files[0].Bytes);
        roundTrip.Files[1].Bytes.Should().Equal(container.Files[1].Bytes);
    }

    [Fact]
    public void CstWriter_WritesCastContainer()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x20, 0x21, 0x22 }
        });

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        var roundTrip = context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var format = context.DataBlock!.Format;
        format.Codec.Should().Be(BlRifxCodec.MV93);
        format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);
        roundTrip.Files.Should().ContainSingle();
        roundTrip.Files[0].Bytes.Should().Equal(container.Files[0].Bytes);
    }

    [Fact]
    public void CstWriter_WritesSoundCastLibrary()
    {
        var mp3Bytes = new byte[]
        {
            (byte)'I', (byte)'D', (byte)'3', 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            (byte)'T', (byte)'A', (byte)'G', 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFB, 0x50, 0x80
        };

        var container = BlLegacySoundLibraryBuilder.BuildSingleMemberMp3Library("mp3 member", mp3Bytes);

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var format = context.DataBlock!.Format;
        format.Codec.Should().Be(BlRifxCodec.MV93);
        format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);

        var sounds = context.ReadSounds();
        sounds.Should().ContainSingle();
        var sound = sounds[0];
        sound.Format.Should().Be(BlLegacySoundFormatKind.Mp3);
        sound.Bytes.Should().Equal(mp3Bytes);
    }

    [Fact]
    public void DctWriter_WritesProtectedMovie()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x30, 0x31 }
        });

        var writer = new BlDctFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dct", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        context.DataBlock!.Format.Codec.Should().Be(BlRifxCodec.MC95);
    }

    [Fact]
    public void LegacyWriter_SelectsWriterFromExtension()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x40 }
        });

        var writer = new BlLegacyWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, "library.cst", container, leaveOpen: true);

        stream.CanWrite.Should().BeTrue();

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        context.DataBlock!.Format.Codec.Should().Be(BlRifxCodec.MV93);
    }
}
