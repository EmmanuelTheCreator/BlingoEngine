using System;
using System.IO;

using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Texts;

/// <summary>
/// Encodes synthetic text resources for test scenarios. The writer mirrors the classic eight-byte
/// chunk prefix described in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> — a
/// four-character tag followed by a big-endian payload length — so
/// <see cref="BlClassicPayloadLoader"/> can resolve the bytes across every Director generation.
/// </summary>
internal sealed class BlLegacyTextWriter
{
    private static readonly BlTag StxtTag = BlTag.Get("STXT");
    private static readonly BlTag XmedTag = BlTag.Get("XMED");

    private readonly BlStreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyTextWriter"/> class.
    /// </summary>
    /// <param name="stream">Destination stream that receives the text chunks.</param>
    public BlLegacyTextWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    /// <summary>
    /// Writes a classic <c>STXT</c> chunk that Director 2–10 projectors associate with static text
    /// and editable fields. The payload typically contains MacRoman characters or Pascal strings as
    /// captured in the byte tables documented for those versions.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw <c>STXT</c> bytes copied from the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> pointing at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteStxt(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, StxtTag, payload);

    /// <summary>
    /// Writes a styled <c>XMED</c> chunk that later Director versions emit alongside cast headers.
    /// The payload includes style runs and colour tables laid out according to the offsets captured
    /// in <c>XMED_Offsets.md</c>.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw <c>XMED</c> bytes preserved for downstream decoding.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> pointing at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteXmed(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, XmedTag, payload);

    private BlLegacyResourceEntry WriteResource(int resourceId, BlTag tag, ReadOnlySpan<byte> payload)
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
