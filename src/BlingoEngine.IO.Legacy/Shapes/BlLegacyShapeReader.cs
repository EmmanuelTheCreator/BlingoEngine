using System;
using System.Buffers.Binary;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Shapes;

/// <summary>
/// Reads shape cast-member records from the resource table. The reader resolves each <c>CASt</c>
/// entry, inflates compressed bodies when necessary, and slices out the 17-byte QuickDraw payload
/// described in docs/DirDissasembly/ScummVm/Shapes.md so higher layers can inspect the ink and colour
/// flags.
/// </summary>
internal sealed class BlLegacyShapeReader
{
    private const int ShapeRecordLength = 17;
    private const int ModernHeaderLength = 12;
    private const int TransitionalHeaderLength = 7;

    private static readonly BlTag CastTag = BlTag.Cast;

    private readonly ReaderContext _context;

    public BlLegacyShapeReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <summary>
    /// Reads every shape record referenced by the resource tables, returning the raw bytes alongside
    /// a best-effort format classification that explains how to interpret the colour components.
    /// </summary>
    public IReadOnlyList<BlLegacyShape> Read()
    {
        var shapes = new List<BlLegacyShape>();
        if (_context.Resources.Entries.Count == 0)
        {
            return shapes;
        }

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        var movieFormat = _context.DataBlock?.Format;
        bool isBigEndian = movieFormat?.IsBigEndian ?? true;

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != CastTag)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                continue;
            }

            if (!TryExtractShape(payload, isBigEndian, out var parsedFormat, out var bytes))
            {
                continue;
            }

            var format = BlLegacyShapeFormat.Resolve(movieFormat, parsedFormat);
            shapes.Add(new BlLegacyShape(entry.Id, format, bytes));
        }

        return shapes;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private static bool TryExtractShape(byte[] payload, bool isBigEndian, out BlLegacyShapeFormatKind format, out byte[] bytes)
    {
        var span = payload.AsSpan();

        if (TryParseModern(span, isBigEndian, out var offset, out var length))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            bytes = span.Slice(offset, length).ToArray();
            return true;
        }

        if (TryParseTransitional(span, isBigEndian, out offset, out length))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            bytes = span.Slice(offset, length).ToArray();
            return true;
        }

        if (TryParseVintage(span, out offset, out length))
        {
            format = BlLegacyShapeFormatKind.Director2To3SignedColors;
            bytes = span.Slice(offset, length).ToArray();
            return true;
        }

        format = BlLegacyShapeFormatKind.Unknown;
        bytes = Array.Empty<byte>();
        return false;
    }

    private static bool TryParseModern(ReadOnlySpan<byte> payload, bool isBigEndian, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < ModernHeaderLength)
        {
            return false;
        }

        uint castType = ReadUInt32(payload, isBigEndian);
        if (castType != 8)
        {
            return false;
        }

        uint infoSize = ReadUInt32(payload.Slice(4, 4), isBigEndian);
        uint dataSize = ReadUInt32(payload.Slice(8, 4), isBigEndian);
        if (dataSize == 0)
        {
            return false;
        }

        if (infoSize > (uint)(payload.Length - ModernHeaderLength))
        {
            return false;
        }

        int dataOffset = ModernHeaderLength + (int)infoSize;
        if (dataOffset < 0 || dataOffset > payload.Length)
        {
            return false;
        }

        int available = payload.Length - dataOffset;
        if (available <= 0)
        {
            return false;
        }

        if (dataSize > (uint)available)
        {
            dataSize = (uint)available;
        }

        if (dataSize < ShapeRecordLength)
        {
            return false;
        }

        offset = dataOffset;
        length = (int)dataSize;
        return true;
    }

    private static bool TryParseTransitional(ReadOnlySpan<byte> payload, bool isBigEndian, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < TransitionalHeaderLength)
        {
            return false;
        }

        ushort castDataSize = ReadUInt16(payload, isBigEndian);
        uint infoSize = ReadUInt32(payload.Slice(2, 4), isBigEndian);
        byte castType = payload[6];
        if (castType != 8)
        {
            return false;
        }

        if (infoSize > (uint)(payload.Length - TransitionalHeaderLength))
        {
            return false;
        }

        int afterHeader = payload.Length - TransitionalHeaderLength;
        int castDataAreaLength = afterHeader - (int)infoSize;
        if (castDataAreaLength <= 0)
        {
            return false;
        }

        int declaredAreaLength = Math.Max(0, castDataSize - 1);
        int availableAreaLength = Math.Min(declaredAreaLength, castDataAreaLength);
        if (availableAreaLength < ShapeRecordLength)
        {
            return false;
        }

        int dataAreaOffset = TransitionalHeaderLength + (castDataAreaLength - availableAreaLength);
        int recordOffset = dataAreaOffset + (availableAreaLength - ShapeRecordLength);
        if (recordOffset < dataAreaOffset || recordOffset + ShapeRecordLength > payload.Length)
        {
            return false;
        }

        offset = recordOffset;
        length = ShapeRecordLength;
        return true;
    }

    private static bool TryParseVintage(ReadOnlySpan<byte> payload, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < 2)
        {
            return false;
        }

        byte entrySize = payload[0];
        if (entrySize < 2)
        {
            return false;
        }

        byte castType = payload[1];
        if (castType != 8)
        {
            return false;
        }

        int available = Math.Min(entrySize - 1, payload.Length - 2);
        if (available < ShapeRecordLength)
        {
            return false;
        }

        int recordOffset = 2 + (available - ShapeRecordLength);
        if (recordOffset < 2 || recordOffset + ShapeRecordLength > payload.Length)
        {
            return false;
        }

        offset = recordOffset;
        length = ShapeRecordLength;
        return true;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data, bool isBigEndian)
        => isBigEndian ? BinaryPrimitives.ReadUInt32BigEndian(data) : BinaryPrimitives.ReadUInt32LittleEndian(data);

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, bool isBigEndian)
        => isBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(data) : BinaryPrimitives.ReadUInt16LittleEndian(data);
}

internal static class BlLegacyShapeReaderExtensions
{
    public static IReadOnlyList<BlLegacyShape> ReadShapes(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyShapeReader(context);
        return reader.Read();
    }
}
