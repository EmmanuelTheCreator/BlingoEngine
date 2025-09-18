using System;
using System.IO;

using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Encodes synthetic bitmap resources for test scenarios. The writer mirrors the classic
/// eight-byte chunk prefix described in <c>docs/LegacyBitmapLoading.md</c> — a four-character tag
/// followed by a big-endian payload length — so <see cref="BlClassicPayloadLoader"/> can resolve the
/// bytes using the legacy resource map across every Director generation.
/// </summary>
internal sealed class BlLegacyBitmapWriter
{
    private static readonly BlTag BitdTag = BlTag.Register("BITD");
    private static readonly BlTag DibTag = BlTag.Register("DIB ");
    private static readonly BlTag PictTag = BlTag.Register("PICT");
    private static readonly BlTag EditorTag = BlTag.Register("ediM");
    private static readonly BlTag AlphaTag = BlTag.Register("ALFA");
    private static readonly BlTag ThumbTag = BlTag.Register("Thum");

    private readonly BlStreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyBitmapWriter"/> class.
    /// </summary>
    /// <param name="stream">Destination stream that receives the bitmap chunks.</param>
    public BlLegacyBitmapWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    /// <summary>
    /// Writes a Macintosh BITD bitmap chunk, which older Director (≤ 3) projectors stored in the
    /// resource map. The payload should contain the run-length encoded pixel rows documented in the
    /// legacy byte tables.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw BITD bytes (control stream followed by literal/repeat data).</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteBitd(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, BitdTag, payload);

    /// <summary>
    /// Writes a Windows <c>DIB </c> resource that Director 4/5 emitted alongside the classic BITD
    /// chunk. The payload should begin with one of the BITMAPINFOHEADER sizes (0x0C, 0x28, 0x40,
    /// 0x6C, or 0x7C) so the loader can classify the format correctly.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw DIB bytes including the palette + BITMAPINFOHEADER layout.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteDib(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, DibTag, payload);

    /// <summary>
    /// Writes a QuickDraw <c>PICT</c> chunk used by classic Macintosh projectors when the member
    /// stored a drawing instead of a bitmap.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw PICT bytes copied from the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WritePict(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, PictTag, payload);

    /// <summary>
    /// Writes an authoring metadata (<c>ediM</c>) chunk that can embed PNG/JPEG/DIB bytes in modern
    /// movies. Tests can feed either valid image signatures or arbitrary metadata to exercise the
    /// fallback paths documented for Director 6+.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw metadata bytes, typically starting with a modern image signature.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteEditorMetadata(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, EditorTag, payload);

    /// <summary>
    /// Writes an <c>ALFA</c> chunk representing the standalone alpha mask that Director 5+ stored next
    /// to bitmap members.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw alpha bytes recorded in the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteAlphaMask(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, AlphaTag, payload);

    /// <summary>
    /// Writes the <c>Thum</c> thumbnail payload produced by newer authoring tools alongside the
    /// primary bitmap.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw thumbnail bytes to persist inside the chunk.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteThumbnail(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, ThumbTag, payload);

    /// <summary>
    /// Writes a resource chunk using the standard 8-byte prefix (tag + big-endian length) followed
    /// by the raw payload bytes. The returned entry can be registered in a
    /// <see cref="ReaderContext"/> so unit tests mimic the memory-map layout used by Director.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="tag">Four-character code associated with the payload.</param>
    /// <param name="payload">Raw payload bytes to write after the chunk header.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteResource(int resourceId, BlTag tag, ReadOnlySpan<byte> payload)
    {
        if (resourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(resourceId), "Resource identifiers must be positive.");

        var payloadLength = payload.Length;
        var declaredSize = checked((uint)payloadLength);

        var offset = _writer.Position;
        if (offset < 0 || offset > uint.MaxValue)
            throw new InvalidOperationException("Chunk offset exceeds the range supported by legacy maps.");

        _writer.WriteTag(tag);
        _writer.WriteUInt32(declaredSize);
        _writer.WriteBytes(payload);

        return new BlLegacyResourceEntry(resourceId, tag, declaredSize, (uint)offset, 0, 0, 0);
    }
}
