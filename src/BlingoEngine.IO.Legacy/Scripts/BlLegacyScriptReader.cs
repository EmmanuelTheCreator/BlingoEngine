using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    private static readonly BlTag ScriptContextLowerTag = BlTag.Register("Lctx");
    private static readonly BlTag ScriptContextUpperTag = BlTag.Register("LctX");


    private readonly ReaderContext _context;

    public BlLegacyScriptReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacyScript> Read()
    {
        var scripts = new List<BlLegacyScript>();
        if (_context.Resources.Entries.Count == 0) return scripts;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = _context.AfterburnerState is null
            ? null
            : new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);

        var scriptResourceByNumber = BuildScriptResourceMap(classicLoader, afterburnerLoader);
        var descriptors = BuildScriptDescriptors(classicLoader, afterburnerLoader, scriptResourceByNumber);

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != ScriptTag) continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0) continue;

            descriptors.TryGetValue(entry.Id, out var desc);
            var format = desc?.Format ?? BlLegacyScriptFormatKind.Unknown;
            var text = desc?.Text;
            var name = desc?.Name;

            scripts.Add(new BlLegacyScript(entry.Id, format, payload, text, name));
        }

        return scripts;
    }


    private Dictionary<int, int> BuildScriptResourceMap(
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        var result = new Dictionary<int, int>();

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != ScriptContextLowerTag && entry.Tag != ScriptContextUpperTag)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                continue;
            }

            ParseScriptContext(payload, result);
        }

        return result;
    }

    private Dictionary<int, ScriptDescriptor> BuildScriptDescriptors(
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader,
        IReadOnlyDictionary<int, int> scriptResourceByNumber)
    {
        var result = new Dictionary<int, ScriptDescriptor>();
        var entriesById = _context.Resources.EntriesById;
        var directorVersion = _context.DataBlock?.Format.DirectorVersion ?? 0;

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != CastTag && entry.Tag != LegacyCastTag) continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0) continue;

            if (!TryReadScriptDescriptor(payload, entriesById, scriptResourceByNumber,
                directorVersion, out var scriptId, out var format, out var text, out var name))
            {
                continue;
            }

            if (scriptId == 0 || result.ContainsKey(scriptId)) continue;
            result.Add(scriptId, new ScriptDescriptor(format, text, name));
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


    private static void ParseScriptContext(byte[] payload, IDictionary<int, int> scriptResourceByNumber)
    {
        if (payload.Length < 24)
        {
            return;
        }

        using var memory = new MemoryStream(payload, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = BlEndianness.BigEndian
        };

        reader.Skip(sizeof(int) * 2); // Unknown header words
        var entryCount = reader.ReadUInt32();
        var entryCount2 = reader.ReadUInt32();
        var entriesOffset = reader.ReadUInt16();
        reader.Skip(sizeof(short)); // Unknown2
        reader.Skip(sizeof(int) * 3); // Unknown3..Unknown5
        reader.Skip(sizeof(int)); // LnamSectionId
        reader.Skip(sizeof(short) * 3); // ValidCount, Flags, FreePointer

        if (entryCount == 0)
        {
            return;
        }

        var entriesStart = entriesOffset;
        if (entriesStart < 0 || entriesStart >= payload.Length)
        {
            return;
        }

        reader.Position = entriesStart;

        var rawLimit = entryCount2 > 0 ? Math.Min(entryCount, entryCount2) : entryCount;
        var bytesRemaining = payload.Length - entriesStart;
        if (bytesRemaining <= 0)
        {
            return;
        }

        var capacity = (uint)(bytesRemaining / 12);
        var entryLimit = (int)Math.Min(rawLimit, capacity);
        for (var index = 0; index < entryLimit; index++)
        {
            reader.Skip(sizeof(int)); // Unknown entry field
            var sectionId = reader.ReadInt32();
            reader.Skip(sizeof(ushort) * 2); // Entry padding/flags

            var scriptNumber = index + 1;
            if (sectionId > 0 && !scriptResourceByNumber.ContainsKey(scriptNumber))
            {
                scriptResourceByNumber.Add(scriptNumber, sectionId);
            }
        }
    }

    private static bool TryReadScriptDescriptor(
        byte[] payload,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById,
        IReadOnlyDictionary<int, int> scriptResourceByNumber,
        int directorVersion,

        out int scriptResourceId,
        out BlLegacyScriptFormatKind format,
        out string? scriptText,
        out string? scriptName)
    {
        scriptResourceId = 0;
        format = BlLegacyScriptFormatKind.Unknown;
        scriptText = null;
        scriptName = null;

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

        var infoAvail = payload.Length - (int)reader.Position;
        if (infoLength > (uint)infoAvail) infoLength = (uint)infoAvail;

        var infoBytes = infoLength > 0 ? reader.ReadBytes((int)infoLength) : Array.Empty<byte>();
        var infoSpan = infoBytes.AsSpan();

        var specAvail = payload.Length - (int)reader.Position;
        if (specificLength > (uint)specAvail) specificLength = (uint)specAvail;

        var specificData = specificLength > 0 ? reader.ReadBytes((int)specificLength) : Array.Empty<byte>();

        scriptResourceId = ResolveScriptResourceId(infoBytes, entriesById, scriptResourceByNumber);

        format = BlLegacyScriptFormat.Detect(specificData);

        if (!TryExtractScriptContent(infoSpan, directorVersion, out scriptText, out scriptName))
            return scriptResourceId != 0;

        return scriptResourceId != 0;
    }

    private static bool TryExtractScriptContent(
        ReadOnlySpan<byte> infoData,
        int directorVersion,
        out string? scriptText,
        out string? scriptName)
    {
        scriptText = null;
        scriptName = null;

        const int textLenOffset = 0x1D;
        const int textLenSize = 4;
        const int pointerTextStart = 0x6A;
        const int legacyTextStart = textLenOffset + textLenSize;

        if (infoData.Length < textLenOffset + textLenSize)
        {
            return false;
        }

        var rawLength = BinaryPrimitives.ReadUInt32LittleEndian(infoData.Slice(textLenOffset, textLenSize));
        if (rawLength == 0 || rawLength > int.MaxValue)
        {
            return false;
        }

        var textLength = (int)rawLength;
        if (textLength > infoData.Length)
        {
            return false;
        }

        var primaryStart = directorVersion >= 4 ? pointerTextStart : legacyTextStart;
        var alternateStart = directorVersion >= 4 ? legacyTextStart : pointerTextStart;

        var textStart = primaryStart;
        if (textStart < 0 || textLength > infoData.Length - textStart)
        {
            textStart = alternateStart;
            if (textStart < 0 || textLength > infoData.Length - textStart)
            {
                return false;
            }
        }

        var textEnd = textStart + textLength;
        scriptText = Encoding.Latin1.GetString(infoData.Slice(textStart, textLength));

        if (textEnd >= infoData.Length)
        {
            return true;
        }

        var nameLen = infoData[textEnd];
        if (nameLen == 0)
        {
            return true;
        }

        var nameStart = textEnd + 1;
        var nameEnd = nameStart + nameLen;
        if (nameEnd > infoData.Length)
        {
            return true;
        }

        scriptName = Encoding.Latin1.GetString(infoData.Slice(nameStart, nameLen));
        return true;
    }

    private sealed class ScriptDescriptor
    {
        public ScriptDescriptor(BlLegacyScriptFormatKind format, string? text, string? name)
        {
            Format = format;
            Text = text;
            Name = name;
        }

        public BlLegacyScriptFormatKind Format { get; }

        public string? Text { get; }

        public string? Name { get; }
    }


    private static int ResolveScriptNumber(byte[] infoData)
    {
        if (infoData.Length < 20)
        {
            return 0;
        }

        var span = infoData.AsSpan(16, 4);
        var value = BinaryPrimitives.ReadInt32BigEndian(span);
        return value > 0 ? value : 0;
    }

    private static int ResolveScriptResourceId(
        byte[] infoData,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById,
        IReadOnlyDictionary<int, int> scriptResourceByNumber)
    {
        var scriptNumber = ResolveScriptNumber(infoData);
        if (scriptNumber > 0 && scriptResourceByNumber.TryGetValue(scriptNumber, out var mappedId) && entriesById.ContainsKey(mappedId))
        {
            return mappedId;
        }


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
