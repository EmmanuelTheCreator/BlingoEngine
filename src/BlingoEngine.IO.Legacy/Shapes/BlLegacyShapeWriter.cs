using System;
using System.IO;

using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Shapes;

/// <summary>
/// Provides helpers for emitting legacy shape cast-member payloads so tests can synthesise
/// <c>CASt</c> bodies for every Director generation. Each method mirrors the wrappers
/// documented in <c>docs/LegacyShapeRecords.md</c> and preserves the 17-byte QuickDraw
/// record described in the table:
/// <list type="bullet">
/// <item><description><c>0x00-0x01</c> – Shape enumeration.</description></item>
/// <item><description><c>0x02-0x09</c> – Bounding rectangle (top, left, bottom, right).</description></item>
/// <item><description><c>0x0A-0x0B</c> – Fill pattern identifier.</description></item>
/// <item><description><c>0x0C</c> – Foreground colour byte.</description></item>
/// <item><description><c>0x0D</c> – Background colour byte.</description></item>
/// <item><description><c>0x0E</c> – Fill and ink flags.</description></item>
/// <item><description><c>0x0F</c> – Pen thickness.</description></item>
/// <item><description><c>0x10</c> – Pattern direction byte.</description></item>
/// </list>
/// </summary>
internal static class BlLegacyShapeWriter
{
    private const int ShapeRecordLength = 17;
    private const byte ShapeCastType = 8;

    /// <summary>
    /// Wraps a shape record using the Director 2–3 <c>VWCR</c> layout. The encoded bytes follow the
    /// ordering detailed in the documentation:
    /// <list type="bullet">
    /// <item><description><c>0x00</c> – Entry length including the type byte and optional flag.</description></item>
    /// <item><description><c>0x01</c> – Cast type selector; <c>8</c> identifies shape members.</description></item>
    /// <item><description><c>0x02</c> – Optional <paramref name="flags"/> byte retained for parity with projector output.</description></item>
    /// <item><description><c>0x02/0x03</c> – Start of the 17-byte QuickDraw payload.</description></item>
    /// </list>
    /// </summary>
    /// <param name="shapeRecord">The 17-byte QuickDraw structure to emit verbatim.</param>
    /// <param name="includeFlags">Whether to write the legacy <c>flags1</c> byte before the record.</param>
    /// <param name="flags">Flag value stored at offset <c>0x02</c> when <paramref name="includeFlags"/> is <see langword="true"/>.</param>
    /// <returns>The legacy <c>VWCR</c> entry that precedes the shape payload.</returns>
    public static byte[] BuildVintageEntry(ReadOnlySpan<byte> shapeRecord, bool includeFlags = true, byte flags = 0)
    {
        ValidateShapeRecord(shapeRecord);

        int entrySize = 1 + shapeRecord.Length + (includeFlags ? 1 : 0);
        if (entrySize > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(shapeRecord), "Legacy VWCR entries cannot exceed 255 bytes.");
        }

        using var stream = new MemoryStream(entrySize + 2);
        var writer = new BlStreamWriter(stream);
        writer.Endianness = BlEndianness.BigEndian;
        writer.WriteByte((byte)entrySize);
        writer.WriteByte(ShapeCastType);
        if (includeFlags)
        {
            writer.WriteByte(flags);
        }

        writer.WriteBytes(shapeRecord);
        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>
    /// Wraps a shape record using the Director 4 transitional <c>CASt</c> header. The emitted bytes align with
    /// the table in <c>docs/LegacyShapeRecords.md</c>:
    /// <list type="bullet">
    /// <item><description><c>0x00-0x01</c> – Cast-data size including the type byte.</description></item>
    /// <item><description><c>0x02-0x05</c> – Cast-info size that trails the data section.</description></item>
    /// <item><description><c>0x06</c> – Cast type selector (<c>8</c> for shapes).</description></item>
    /// <item><description><c>0x07</c> – Optional <paramref name="flags"/> byte preserved from the original file.</description></item>
    /// <item><description><c>0x07/0x08</c> – Start of the 17-byte QuickDraw payload.</description></item>
    /// <item><description>Cast-info bytes follow immediately after the payload.</description></item>
    /// </list>
    /// </summary>
    /// <param name="shapeRecord">The 17-byte QuickDraw structure to emit verbatim.</param>
    /// <param name="includeFlags">Whether to include the legacy <c>flags1</c> byte in the cast-data section.</param>
    /// <param name="flags">Flag value stored at offset <c>0x07</c> when <paramref name="includeFlags"/> is <see langword="true"/>.</param>
    /// <param name="infoBytes">Optional metadata bytes appended after the cast-data payload.</param>
    /// <returns>The transitional <c>CASt</c> payload that precedes the shape record.</returns>
    public static byte[] BuildTransitionalEntry(ReadOnlySpan<byte> shapeRecord, bool includeFlags = true, byte flags = 0, byte[]? infoBytes = null)
    {
        ValidateShapeRecord(shapeRecord);

        var info = infoBytes ?? Array.Empty<byte>();
        int castDataPayloadLength = shapeRecord.Length + (includeFlags ? 1 : 0);
        int castDataSize = 1 + castDataPayloadLength;
        if (castDataSize > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(shapeRecord), "Cast-data sections must fit in 16 bits for Director 4 headers.");
        }

        using var stream = new MemoryStream();
        var writer = new BlStreamWriter(stream);
        writer.Endianness = BlEndianness.BigEndian;
        writer.WriteUInt16((ushort)castDataSize);
        writer.WriteUInt32((uint)info.Length);
        writer.WriteByte(ShapeCastType);
        if (includeFlags)
        {
            writer.WriteByte(flags);
        }

        writer.WriteBytes(shapeRecord);
        if (info.Length > 0)
        {
            writer.WriteBytes(info);
        }

        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>
    /// Wraps a shape record using the Director 5–10 <c>CASt</c> header. The encoded bytes match the documented
    /// layout where the four-byte cast type and info length precede the QuickDraw payload.
    /// </summary>
    /// <param name="shapeRecord">The 17-byte QuickDraw structure to emit verbatim.</param>
    /// <param name="infoBytes">Optional metadata appended between the header and the shape payload.</param>
    /// <returns>The modern <c>CASt</c> payload that precedes the QuickDraw record.</returns>
    public static byte[] BuildModernEntry(ReadOnlySpan<byte> shapeRecord, byte[]? infoBytes = null)
    {
        ValidateShapeRecord(shapeRecord);

        var info = infoBytes ?? Array.Empty<byte>();

        using var stream = new MemoryStream();
        var writer = new BlStreamWriter(stream);
        writer.Endianness = BlEndianness.BigEndian;
        writer.WriteUInt32(ShapeCastType);
        writer.WriteUInt32((uint)info.Length);
        writer.WriteUInt32((uint)shapeRecord.Length);
        if (info.Length > 0)
        {
            writer.WriteBytes(info);
        }

        writer.WriteBytes(shapeRecord);
        writer.Flush();
        return stream.ToArray();
    }

    private static void ValidateShapeRecord(ReadOnlySpan<byte> shapeRecord)
    {
        if (shapeRecord.Length != ShapeRecordLength)
        {
            throw new ArgumentException($"Shape records must contain exactly {ShapeRecordLength} bytes.", nameof(shapeRecord));
        }
    }
}
