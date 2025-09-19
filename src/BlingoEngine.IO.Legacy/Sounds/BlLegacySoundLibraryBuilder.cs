using System;
using System.Buffers.Binary;
using System.Text;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Provides helpers for synthesising minimal cast libraries that embed MP3 sound members. The
/// generated resources mirror the <c>KEY*</c>, <c>CAS*</c>, <c>CASt</c>, and <c>ediM</c> layout used by
/// modern Director authoring movies so the resulting container can be consumed by the existing
/// reader pipeline.
/// </summary>
public static class BlLegacySoundLibraryBuilder
{
    private const uint CastMemberTypeSound = 6;
    private const uint KeyResourceId = 1;
    private const uint CastTableResourceId = 2;
    private const uint CastMemberResourceId = 3;
    private const uint EditorResourceId = 4;

    private static readonly BlTag EditorTag = BlTag.Register("ediM");

    /// <summary>
    /// Builds a <see cref="DirFilesContainerDTO"/> that contains a single sound cast member backed by
    /// the supplied MP3 payload. The generated container is ready to be persisted through the
    /// <see cref="BlCstFileWriter"/>.
    /// </summary>
    /// <param name="memberName">Display name stored in the <c>CASt</c> metadata.</param>
    /// <param name="mp3Bytes">Raw MP3 bytes that populate the <c>ediM</c> chunk.</param>
    /// <returns>A container that exposes the resources necessary to emit a cast library.</returns>
    public static DirFilesContainerDTO BuildSingleMemberMp3Library(string? memberName, ReadOnlySpan<byte> mp3Bytes)
    {
        if (mp3Bytes.IsEmpty)
        {
            throw new ArgumentException("MP3 payload must contain at least one byte.", nameof(mp3Bytes));
        }

        if (mp3Bytes.Length > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(mp3Bytes), "MP3 payload exceeds the supported size for legacy archives.");
        }

        var container = new DirFilesContainerDTO();

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.KeyStar.Value}_{KeyResourceId:D4}.bin",
            Bytes = BuildKeyTable()
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.CasStar.Value}_{CastTableResourceId:D4}.bin",
            Bytes = BuildCastTable()
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"CASt_{CastMemberResourceId:D4}.bin",
            Bytes = BuildCastMember(memberName)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"ediM_{EditorResourceId:D4}.bin",
            Bytes = mp3Bytes.ToArray()
        });

        return container;
    }

    private static byte[] BuildKeyTable()
    {
        Span<byte> payload = stackalloc byte[24];
        BinaryPrimitives.WriteUInt16LittleEndian(payload[0..2], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(payload[2..4], 12);
        BinaryPrimitives.WriteUInt32LittleEndian(payload[4..8], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(payload[8..12], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(payload[12..16], EditorResourceId);
        BinaryPrimitives.WriteUInt32LittleEndian(payload[16..20], CastMemberResourceId);
        BinaryPrimitives.WriteUInt32LittleEndian(payload[20..24], EditorTag.ToUInt32BigEndian());
        return payload.ToArray();
    }

    private static byte[] BuildCastTable()
    {
        var payload = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(payload, CastMemberResourceId);
        return payload;
    }

    private static byte[] BuildCastMember(string? memberName)
    {
        var name = memberName ?? string.Empty;
        var nameBytes = Encoding.UTF8.GetBytes(name);
        if (nameBytes.Length > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(memberName), "Cast-member names must fit in a single length byte.");
        }

        int infoLength = 1 + nameBytes.Length;
        var payload = new byte[12 + infoLength];

        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(0, 4), CastMemberTypeSound);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)infoLength);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), 0u);

        payload[12] = (byte)nameBytes.Length;
        if (nameBytes.Length > 0)
        {
            nameBytes.CopyTo(payload.AsSpan(13));
        }

        return payload;
    }
}
