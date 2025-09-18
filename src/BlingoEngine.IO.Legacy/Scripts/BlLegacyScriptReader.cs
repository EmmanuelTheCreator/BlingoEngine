using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scripts;

/// <summary>
/// Reads compiled <c>Lscr</c> resources and associates them with their script
/// category by inspecting the owning <c>CASt</c> records. Director stores the
/// selector word in the member-specific data while the info block records the
/// script resource id, so the reader walks both regions to build a typed list of
/// compiled scripts.
/// </summary>
internal sealed class BlLegacyScriptReader
{
    private static readonly BlTag ScriptTag = BlTag.Register("Lscr");
    private static readonly BlTag CastTag = BlTag.Cast;
    private static readonly BlTag LegacyCastTag = BlTag.Register("CASt");

    private readonly ReaderContext _context;

    public BlLegacyScriptReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacyScript> Read()
    {
        var scripts = new List<BlLegacyScript>();
        if (_context.Resources.Entries.Count == 0)
        {
            return scripts;
        }

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        var formatByResourceId = BuildScriptFormatMap(classicLoader, afterburnerLoader);

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != ScriptTag)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                continue;
            }

            formatByResourceId.TryGetValue(entry.Id, out var format);
            scripts.Add(new BlLegacyScript(entry.Id, format, payload));
        }

        return scripts;
    }

    private Dictionary<int, BlLegacyScriptFormatKind> BuildScriptFormatMap(
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        var result = new Dictionary<int, BlLegacyScriptFormatKind>();
        var entriesById = _context.Resources.EntriesById;

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != CastTag && entry.Tag != LegacyCastTag)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                continue;
            }

            if (!TryReadScriptDescriptor(payload, entriesById, out var scriptId, out var format))
            {
                continue;
            }

            if (scriptId == 0)
            {
                continue;
            }

            if (!result.ContainsKey(scriptId))
            {
                result.Add(scriptId, format);
            }
        }

        return result;
    }

    private static byte[] LoadPayload(
        BlLegacyResourceEntry entry,
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private static bool TryReadScriptDescriptor(
        byte[] payload,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById,
        out int scriptResourceId,
        out BlLegacyScriptFormatKind format)
    {
        scriptResourceId = 0;
        format = BlLegacyScriptFormatKind.Unknown;

        if (payload.Length < 12)
        {
            return false;
        }

        using var memory = new MemoryStream(payload, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = BlEndianness.BigEndian
        };

        var memberType = reader.ReadUInt32();
        if (memberType != 11)
        {
            return false;
        }

        var infoLength = reader.ReadUInt32();
        var specificLength = reader.ReadUInt32();

        var infoBytesAvailable = payload.Length - (int)reader.Position;
        if (infoBytesAvailable <= 0)
        {
            return false;
        }

        if (infoLength > (uint)infoBytesAvailable)
        {
            infoLength = (uint)infoBytesAvailable;
        }

        var infoData = infoLength > 0 ? reader.ReadBytes((int)infoLength) : Array.Empty<byte>();

        var remaining = payload.Length - (int)reader.Position;
        if (remaining <= 0)
        {
            specificLength = 0;
        }
        else if (specificLength > (uint)remaining)
        {
            specificLength = (uint)remaining;
        }

        var specificData = specificLength > 0 ? reader.ReadBytes((int)specificLength) : Array.Empty<byte>();

        scriptResourceId = ResolveScriptResourceId(infoData, entriesById);
        format = BlLegacyScriptFormat.Detect(specificData);

        return scriptResourceId != 0;
    }

    private static int ResolveScriptResourceId(
        byte[] infoData,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById)
    {
        if (infoData.Length < 12)
        {
            return 0;
        }

        var span = infoData.AsSpan(8);
        if (span.Length < 4)
        {
            return 0;
        }

        var bigEndianValue = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(span));
        if (bigEndianValue != 0 && entriesById.ContainsKey(bigEndianValue))
        {
            return bigEndianValue;
        }

        var littleEndianValue = unchecked((int)BinaryPrimitives.ReadUInt32LittleEndian(span));
        if (littleEndianValue != 0 && entriesById.ContainsKey(littleEndianValue))
        {
            return littleEndianValue;
        }

        return 0;
    }
}

/// <summary>
/// Extension helpers exposing <see cref="BlLegacyScriptReader"/> through the
/// shared <see cref="ReaderContext"/> used by the legacy pipeline.
/// </summary>
internal static class BlLegacyScriptReaderExtensions
{
    public static IReadOnlyList<BlLegacyScript> ReadScripts(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyScriptReader(context);
        return reader.Read();
    }
}
