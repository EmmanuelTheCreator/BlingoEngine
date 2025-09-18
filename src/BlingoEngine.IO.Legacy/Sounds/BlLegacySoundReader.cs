using System;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Reads sound payloads from the resource table, returning the decoded bytes alongside a
/// best-effort format classification. Director stores the actual audio stream in <c>ediM</c>
/// chunks, so the reader simply resolves those entries, inflates them when necessary, and runs a
/// header inspection on the resulting buffer.
/// </summary>
internal sealed class BlLegacySoundReader
{
    private static readonly BlTag SoundDataTag = BlTag.Register("ediM");

    private readonly ReaderContext _context;

    public BlLegacySoundReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacySound> Read()
    {
        var sounds = new List<BlLegacySound>();
        if (_context.Resources.Entries.Count == 0)
            return sounds;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != SoundDataTag)
                continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
                continue;

            var format = BlLegacySoundFormat.Detect(payload);
            sounds.Add(new BlLegacySound(entry.Id, format, payload));
        }

        return sounds;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }
}

internal static class BlLegacySoundReaderExtensions
{
    public static IReadOnlyList<BlLegacySound> ReadSounds(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacySoundReader(context);
        return reader.Read();
    }
}
