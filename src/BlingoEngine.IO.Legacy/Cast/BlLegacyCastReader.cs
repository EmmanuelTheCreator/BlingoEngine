using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Reads <c>CAS*</c> resources and exposes the cast-member tables they contain. Each table stores a
/// packed list of 32-bit identifiers that point to individual <c>CASt</c> resources. The loader keeps
/// the slot index for every populated entry so higher layers can reconstruct cast numbering.
/// </summary>
internal sealed class BlLegacyCastReader
{
    private readonly ReaderContext _context;

    public BlLegacyCastReader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Iterates over all registered <c>CAS*</c> resources, loading their payload bytes and decoding the
    /// cast-member tables contained within. The reader respects both classic chunk storage and
    /// Afterburner inline segments, inflating compressed data when necessary.
    /// </summary>
    public IReadOnlyList<BlLegacyCastLibrary> Read()
    {
        var libraries = new List<BlLegacyCastLibrary>();
        if (_context.Resources.Entries.Count == 0)
        {
            return libraries;
        }

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != BlTag.CasStar)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            var library = ParseLibrary(entry, payload, classicLoader, afterburnerLoader);
            libraries.Add(library);
        }

        return libraries;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private BlLegacyCastLibrary ParseLibrary(BlLegacyResourceEntry entry, byte[] payload, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        int? parentId = null;
        if (_context.Resources.ParentByChild.TryGetValue(entry.Id, out var link))
        {
            parentId = link.ParentId;
        }

        var entryCount = payload.Length / 4;
        var library = new BlLegacyCastLibrary(entry.Id, parentId, entryCount);
        if (entryCount == 0)
        {
            return library;
        }

        var reader = _context.CreateMemoryReader(payload, BlEndianness.BigEndian);
        for (var slot = 0; slot < entryCount; slot++)
        {
            var castResourceId = unchecked((int)reader.ReadUInt32());
            if (castResourceId == 0)
            {
                continue;
            }

            library.Members.Add(CreateMember(slot, castResourceId, classicLoader, afterburnerLoader));
        }

        return library;
    }

    private BlLegacyCastMember CreateMember(int slot, int resourceId, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        var memberType = BlLegacyCastMemberType.Unknown;
        var name = string.Empty;

        if (_context.Resources.TryGetEntry(resourceId, out var memberEntry))
        {
            var memberPayload = LoadPayload(memberEntry, classicLoader, afterburnerLoader);
            if (memberPayload.Length > 0 && TryParseMemberChunk(memberPayload, out var parsedType, out var parsedName))
            {
                memberType = parsedType;
                if (!string.IsNullOrEmpty(parsedName))
                {
                    name = parsedName;
                }
            }
        }

        return new BlLegacyCastMember(slot, resourceId, memberType, name);
    }

    private static bool TryParseMemberChunk(byte[] payload, out BlLegacyCastMemberType memberType, out string name)
    {
        memberType = BlLegacyCastMemberType.Unknown;
        name = string.Empty;

        if (payload.Length < 12)
        {
            return false;
        }

        using var memory = new MemoryStream(payload, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = BlEndianness.BigEndian
        };

        var typeValue = reader.ReadUInt32();
        memberType = MapMemberType(typeValue);

        var infoLength = reader.ReadUInt32();
        var specificLength = reader.ReadUInt32();

        var infoBytesAvailable = payload.Length - (int)reader.Position;
        if (infoBytesAvailable <= 0)
        {
            return true;
        }

        if (infoLength > (uint)infoBytesAvailable)
        {
            infoLength = (uint)infoBytesAvailable;
        }

        var infoData = infoLength > 0 ? reader.ReadBytes((int)infoLength) : Array.Empty<byte>();

        if (specificLength > 0)
        {
            var skip = Math.Min((int)specificLength, payload.Length - (int)reader.Position);
            if (skip > 0)
            {
                reader.Skip(skip);
            }
        }

        if (infoData.Length > 0)
        {
            var extracted = ExtractMemberName(infoData);
            if (!string.IsNullOrEmpty(extracted))
            {
                name = extracted;
            }
        }

        return true;
    }

    private static BlLegacyCastMemberType MapMemberType(uint value)
    {
        return value switch
        {
            0 => BlLegacyCastMemberType.Null,
            1 => BlLegacyCastMemberType.Bitmap,
            2 => BlLegacyCastMemberType.FilmLoop,
            3 => BlLegacyCastMemberType.Text,
            4 => BlLegacyCastMemberType.Palette,
            5 => BlLegacyCastMemberType.Picture,
            6 => BlLegacyCastMemberType.Sound,
            7 => BlLegacyCastMemberType.Button,
            8 => BlLegacyCastMemberType.Shape,
            9 => BlLegacyCastMemberType.Movie,
            10 => BlLegacyCastMemberType.DigitalVideo,
            11 => BlLegacyCastMemberType.Script,
            12 => BlLegacyCastMemberType.Rte,
            13 => BlLegacyCastMemberType.Font,
            14 => BlLegacyCastMemberType.Xtra,
            15 => BlLegacyCastMemberType.Field,
            _ => BlLegacyCastMemberType.Unknown
        };
    }

    private static string ExtractMemberName(byte[] infoData)
    {
        if (infoData.Length < 10)
        {
            return ScanForPascalString(infoData);
        }

        using var memory = new MemoryStream(infoData, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = BlEndianness.BigEndian
        };

        var _ = reader.ReadUInt32();
        var entryCount = reader.ReadUInt16();
        var itemsLength = reader.ReadUInt32();

        if (entryCount == 0)
        {
            return ScanForPascalString(infoData);
        }

        var offsets = new uint[entryCount];
        for (var i = 0; i < entryCount; i++)
        {
            if (reader.Position > infoData.Length - 4)
            {
                return ScanForPascalString(infoData);
            }

            offsets[i] = reader.ReadUInt32();
        }

        var tableEnd = (int)reader.Position;
        var available = infoData.Length - tableEnd;
        if (available <= 0)
        {
            return ScanForPascalString(infoData);
        }

        var itemsLimit = (int)Math.Min(itemsLength, (uint)available);
        if (itemsLimit <= 0)
        {
            return ScanForPascalString(infoData);
        }

        if (entryCount >= 2)
        {
            var start = (int)Math.Min(offsets[1], (uint)itemsLimit);
            var nextOffset = entryCount > 2 ? offsets[2] : itemsLength;
            var end = (int)Math.Min(nextOffset, (uint)itemsLimit);
            var length = end - start;
            if (start >= 0 && length > 0 && start + length <= itemsLimit)
            {
                var itemsSpan = infoData.AsSpan(tableEnd, itemsLimit);
                var itemSpan = itemsSpan.Slice(start, length);
                var name = ReadPascalString(itemSpan);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
        }

        return ScanForPascalString(infoData);
    }

    private static string ReadPascalString(ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return string.Empty;
        }

        int length = span[0];
        if (length <= 0 || length >= span.Length)
        {
            return string.Empty;
        }

        return Encoding.UTF8.GetString(span.Slice(1, length));
    }

    private static string ScanForPascalString(byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            int length = data[i];
            if (length <= 0 || i + 1 + length > data.Length)
            {
                continue;
            }

            bool ascii = true;
            for (var j = 0; j < length; j++)
            {
                var value = data[i + 1 + j];
                if (value < 0x20 || value > 0x7E)
                {
                    ascii = false;
                    break;
                }
            }

            if (ascii)
            {
                return Encoding.UTF8.GetString(data, i + 1, length);
            }
        }

        return string.Empty;
    }
}

/// <summary>
/// Extension helpers that expose the <see cref="BlLegacyCastReader"/> through the shared
/// <see cref="ReaderContext"/> type used by the legacy pipeline.
/// </summary>
internal static class BlLegacyCastReaderExtensions
{
    public static IReadOnlyList<BlLegacyCastLibrary> ReadCastLibraries(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyCastReader(context);
        return reader.Read();
    }
}
