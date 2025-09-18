using System;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Fields;

/// <summary>
/// Reads editable field payloads referenced by <c>CASt</c> entries. Classic movies store the user
/// facing text in <c>STXT</c> resources, while later releases upgrade to styled <c>XMED</c> chunks.
/// The reader locates both, inflates compressed data when necessary, and exposes the raw bytes
/// together with a lightweight format classification. Byte layouts for each projector generation are
/// captured in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> alongside notes about which flag bits produce
/// editable fields.
/// </summary>
internal sealed class BlLegacyFieldReader
{
    private static readonly BlTag StxtTag = BlTag.Register("STXT");
    private static readonly BlTag XmedTag = BlTag.Register("XMED");

    private static readonly BlTag[] CandidateTags =
    {
        StxtTag,
        XmedTag
    };

    private static readonly HashSet<BlTag> ChildTags = new(CandidateTags);

    private static readonly HashSet<BlTag> StandaloneTags = new(CandidateTags);

    private readonly ReaderContext _context;

    public BlLegacyFieldReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacyField> Read()
    {
        var fields = new List<BlLegacyField>();
        if (_context.Resources.Entries.Count == 0)
            return fields;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);

        var processed = new HashSet<int>();
        var entriesById = _context.Resources.EntriesById;

        foreach (var pair in _context.Resources.ChildrenByParent)
        {
            foreach (var link in pair.Value)
            {
                if (!ChildTags.Contains(link.Tag))
                    continue;

                if (!entriesById.TryGetValue(link.ChildId, out var child))
                    continue;

                if (!processed.Add(child.Id))
                    continue;

                var payload = LoadPayload(child, classicLoader, afterburnerLoader);
                if (payload.Length == 0)
                {
                    processed.Remove(child.Id);
                    continue;
                }

                var format = DetectFormat(child.Tag);
                fields.Add(new BlLegacyField(child.Id, format, payload));
            }
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (!StandaloneTags.Contains(entry.Tag))
                continue;

            if (!processed.Add(entry.Id))
                continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                processed.Remove(entry.Id);
                continue;
            }

            var format = DetectFormat(entry.Tag);
            fields.Add(new BlLegacyField(entry.Id, format, payload));
        }

        return fields;
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

    private static BlLegacyFieldFormatKind DetectFormat(BlTag tag)
    {
        if (tag == StxtTag)
            return BlLegacyFieldFormatKind.Stxt;

        if (tag == XmedTag)
            return BlLegacyFieldFormatKind.Xmed;

        return BlLegacyFieldFormatKind.Unknown;
    }
}

internal static class BlLegacyFieldReaderExtensions
{
    public static IReadOnlyList<BlLegacyField> ReadFields(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyFieldReader(context);
        return reader.Read();
    }
}
