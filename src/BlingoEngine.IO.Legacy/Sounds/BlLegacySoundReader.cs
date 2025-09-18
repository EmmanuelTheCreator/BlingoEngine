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
    private static readonly BlTag EditorTag = BlTag.Register("ediM");
    private static readonly BlTag SoundDirectoryTag = BlTag.Register("snd ");
    private static readonly BlTag SoundSampleTag = BlTag.Register("sndS");
    private static readonly BlTag MacSoundTag = BlTag.Register("SND ");

    private static readonly BlTag[] ChildPriority =
    {
        EditorTag,
        SoundSampleTag,
        MacSoundTag,
        SoundDirectoryTag
    };

    private static readonly HashSet<BlTag> StandaloneTags = new()
    {
        EditorTag,
        SoundSampleTag,
        MacSoundTag,
        SoundDirectoryTag
    };

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

        var processed = new HashSet<int>();
        var entriesById = _context.Resources.EntriesById;

        foreach (var pair in _context.Resources.ChildrenByParent)
        {
            if (!TryLoadChildSound(pair.Value, entriesById, classicLoader, afterburnerLoader, processed, out var sound))
                continue;

            sounds.Add(sound);
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

    private static bool TryLoadChildSound(
        IReadOnlyList<BlResourceKeyLink> links,
        IReadOnlyDictionary<int, BlLegacyResourceEntry> entriesById,
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader,
        HashSet<int> processed,
        out BlLegacySound sound)
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

                var format = BlLegacySoundFormat.Detect(payload);
                processed.Add(child.Id);
                sound = new BlLegacySound(child.Id, format, payload);
                return true;
            }
        }

        sound = null!;
        return false;
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
