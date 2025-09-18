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
/// DIB, PICT, PNG, JPEG, etc.) and may embed authored bitmaps inside <c>ediM</c> resources, so the
/// reader resolves those entries, inflates them when necessary, and classifies the resulting buffer.
/// Byte heuristics mirror the tables captured in <c>docs/LegacyBitmapLoading.md</c>, keeping the
/// loader compatible with classic Director 2 projectors through Director MX.
/// </summary>
internal sealed class BlLegacyBitmapReader
{
    private static readonly BlTag EditorTag = BlTag.Register("ediM");
    private static readonly BlTag BitdTag = BlTag.Register("BITD");
    private static readonly BlTag DibTag = BlTag.Register("DIB ");
    private static readonly BlTag PictTag = BlTag.Register("PICT");
    private static readonly BlTag AlphaTag = BlTag.Register("ALFA");
    private static readonly BlTag ThumbTag = BlTag.Register("Thum");

    private static readonly BlTag[] ChildPriority =
    {
        EditorTag,
        BitdTag,
        DibTag,
        PictTag,
        AlphaTag,
        ThumbTag
    };

    private static readonly HashSet<BlTag> ChildPrioritySet = new(ChildPriority);
    private static readonly HashSet<BlTag> CanonicalBitmapTags = new(ChildPriority);

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
            if (processed.Contains(entry.Id))
                continue;

            if (!ShouldInspectTag(entry.Tag))
                continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
                continue;

            var format = BlLegacyBitmapFormat.Detect(entry.Tag, payload);
            if (!ShouldRetainEntry(entry.Tag, format))
                continue;

            processed.Add(entry.Id);
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
        static bool TryResolve(
            BlLegacyResourceEntry candidate,
            BlClassicPayloadLoader classicLoader,
            BlAfterburnerPayloadLoader? afterburnerLoader,
            HashSet<int> processed,
            out BlLegacyBitmap bitmap)
        {
            bitmap = null!;

            if (processed.Contains(candidate.Id))
                return false;

            var payload = LoadPayload(candidate, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
                return false;

            var format = BlLegacyBitmapFormat.Detect(candidate.Tag, payload);
            if (!ShouldRetainEntry(candidate.Tag, format))
                return false;

            processed.Add(candidate.Id);
            bitmap = new BlLegacyBitmap(candidate.Id, format, payload);
            return true;
        }

        foreach (var tag in ChildPriority)
        {
            for (var i = 0; i < links.Count; i++)
            {
                var link = links[i];
                if (link.Tag != tag)
                    continue;

                if (!entriesById.TryGetValue(link.ChildId, out var child))
                    continue;

                if (TryResolve(child, classicLoader, afterburnerLoader, processed, out bitmap))
                    return true;
            }
        }

        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i];
            if (ChildPrioritySet.Contains(link.Tag))
                continue;

            if (!entriesById.TryGetValue(link.ChildId, out var child))
                continue;

            if (!ShouldInspectTag(child.Tag))
                continue;

            if (TryResolve(child, classicLoader, afterburnerLoader, processed, out bitmap))
                return true;
        }

        bitmap = null!;
        return false;
    }

    private static bool ShouldInspectTag(BlTag tag)
    {
        if (CanonicalBitmapTags.Contains(tag))
            return true;

        var value = tag.Value;

        if (value.StartsWith("PNG", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("JPG", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("JPEG", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("JFIF", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("GIF", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("BMP", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("TIF", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static bool ShouldRetainEntry(BlTag tag, BlLegacyBitmapFormatKind format)
    {
        if (format == BlLegacyBitmapFormatKind.Unknown)
            return tag != EditorTag && CanonicalBitmapTags.Contains(tag);

        if (format == BlLegacyBitmapFormatKind.Dib && tag != DibTag && tag != EditorTag)
            return false;

        if (format == BlLegacyBitmapFormatKind.Pict && tag != PictTag && tag != EditorTag)
            return false;

        if (format == BlLegacyBitmapFormatKind.AlphaMask && tag != AlphaTag)
            return false;

        if (format == BlLegacyBitmapFormatKind.Thumbnail && tag != ThumbTag)
            return false;

        return true;
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
