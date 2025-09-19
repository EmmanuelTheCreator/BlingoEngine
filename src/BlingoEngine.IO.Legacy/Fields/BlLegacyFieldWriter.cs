using System;
using System.IO;

using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Fields;

/// <summary>
/// Encodes editable field resources so tests can emulate Director archives without relying on
/// external sample files. The emitted chunks follow the eight-byte header documented in
/// <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c>, allowing
/// <see cref="BlClassicPayloadLoader"/> to resolve the bytes regardless of the targeted projector
/// version.
/// </summary>
internal sealed class BlLegacyFieldWriter
{
    private static readonly BlTag StxtTag = BlTag.Get("STXT");
    private static readonly BlTag XmedTag = BlTag.Get("XMED");

    private readonly BlStreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyFieldWriter"/> class.
    /// </summary>
    /// <param name="stream">Destination stream that receives the field chunks.</param>
    public BlLegacyFieldWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    /// <summary>
    /// Writes a classic <c>STXT</c> chunk containing the text backing an editable field. Earlier
    /// Director versions relied solely on these plain-text resources, so tests can populate the
    /// payload with any MacRoman or Pascal-style bytes enumerated in the documentation tables.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw <c>STXT</c> bytes copied from the field resource.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> pointing at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteStxt(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, StxtTag, payload);

    /// <summary>
    /// Writes a styled <c>XMED</c> chunk that newer editable fields use to store formatting metadata
    /// alongside the user-visible text stream.
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
