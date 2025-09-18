using System;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Reads bitmap payloads from the resource table, returning the decoded bytes alongside a
/// best-effort format classification. Director stores image data across several chunk types (BITD,
/// DIB, PICT, etc.) and may embed authored bitmaps inside <c>ediM</c> resources, so the reader
/// resolves those entries, inflates them when necessary, and classifies the resulting buffer.
/// </summary>
internal sealed class BlLegacyBitmapReader
{
    private static readonly BlTag EditorTag = BlTag.Register("ediM");
    private static readonly BlTag BitdTag = BlTag.Register("BITD");
    private static readonly BlTag DibTag = BlTag.Register("DIB ");
    private static readonly BlTag PictTag = BlTag.Register("PICT");

    private static readonly BlTag[] ChildPriority =
    {
        EditorTag,
        BitdTag,
        DibTag,
        PictTag
    };

    private static readonly HashSet<BlTag> StandaloneTags = new()
    {
        EditorTag,
        BitdTag,
        DibTag,
        PictTag,
        BlTag.Register("ALFA"),
        BlTag.Register("Thum")
    };

    private readonly ReaderContext _context;

    public BlLegacyBitmapReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacyBitmap> Read()
    {
        var bitmaps = new List<BlLegacyBitmap>();
        if (_context.Resources.Entries.Count == 0)
            return bitmaps;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);

        var processed = new HashSet<int>();
        var entriesById = _context.Resources.EntriesById;

        foreach (var pair in _context.Resources.ChildrenByParent)
        {
            if (!TryLoadChildBitmap(pair.Value, entriesById, classicLoader, afterburnerLoader, processed, out var bitmap))
                continue;

            bitmaps.Add(bitmap);
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

            var format = BlLegacyBitmapFormat.Detect(entry.Tag, payload);
            if (format == BlLegacyBitmapFormatKind.Unknown && entry.Tag == EditorTag)
            {
                processed.Remove(entry.Id);
                continue;
            }

            bitmaps.Add(new BlLegacyBitmap(entry.Id, format, payload));
        }

        return bitmaps;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private static bool TryLoadChildBitmap(
        IReadOnlyList<BlResourceKeyLink> links,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById,
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader,
        HashSet<int> processed,
        out BlLegacyBitmap bitmap)
    {
        foreach (var tag in ChildPriority)
        {
            for (var i = 0; i < links.Count; i++)
            {
                var link = links[i];
                if (link.Tag != tag)
                    continue;

                if (!entriesById.TryGetValue(link.ChildId, out var child))
                    continue;

                if (processed.Contains(child.Id))
                    continue;

                var payload = LoadPayload(child, classicLoader, afterburnerLoader);
                if (payload.Length == 0)
                    continue;

                var format = BlLegacyBitmapFormat.Detect(child.Tag, payload);
                if (format == BlLegacyBitmapFormatKind.Unknown && child.Tag == EditorTag)
                    continue;

                processed.Add(child.Id);
                bitmap = new BlLegacyBitmap(child.Id, format, payload);
                return true;
            }
        }

        bitmap = null!;
        return false;
    }
}

internal static class BlLegacyBitmapReaderExtensions
{
    public static IReadOnlyList<BlLegacyBitmap> ReadBitmaps(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyBitmapReader(context);
        return reader.Read();
    }
}
