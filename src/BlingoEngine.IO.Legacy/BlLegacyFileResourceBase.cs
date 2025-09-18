using System;
using System.Collections.Generic;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Classic.Blocks;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Infrastructure;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Base class for legacy Director file readers. The reader locates the 12-byte <c>RIFX/XFIR</c> header, interprets the map
/// metadata (<c>imap</c>/<c>mmap</c> or <c>ABMP</c>/<c>FGEI</c>), and exposes the parsed resources as DTOs.
/// </summary>
public abstract class BlLegacyFileResourceBase : BlLegacyResourceBase
{
    private BlAfterburnerState? _afterburnerState;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyFileResourceBase"/> class.
    /// </summary>
    protected BlLegacyFileResourceBase(ReaderContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Reads the container and converts the resources into <see cref="DirFileResourceDTO"/> instances.
    /// </summary>
    /// <returns>A DTO container populated with the exported resource bytes.</returns>
    public DirFilesContainerDTO Read()
    {
        Context.ResetRegistries();
        _afterburnerState = null;

        LocateRifx();
        var dataBlock = Context.ReadBlockHeader();

        if (dataBlock.Format.IsAfterburner)
        {
            var afterburner = new BlAfterburnerMapReader(Context);
            var map = afterburner.Read(dataBlock);
            if (!string.IsNullOrEmpty(map.Version))
            {
                dataBlock.Format.AfterburnerVersion = map.Version;
            }

            RegisterCompressions(map.CompressionDescriptors);
            RegisterAfterburnerEntries(map.ResourceEntries);
            RegisterInlineSegments(map.InlineSegments);
            _afterburnerState = map.State;
        }
        else
        {
            var classic = new BlClassicMapReader(Context);
            var map = classic.Read(dataBlock);

            if (map.Imap is not null)
            {
                dataBlock.Format.MapVersion = map.Imap.MapVersion;
                dataBlock.Format.ArchiveVersion = map.Imap.ArchiveVersion;
            }

            if (map.Mmap is not null)
            {
                RegisterClassicEntries(map.Mmap);
            }

            if (map.KeyTable is not null)
            {
                RegisterKeyRelationships(map.KeyTable);
            }
        }

        return BuildContainer();
    }

    /// <summary>
    /// Locates the <c>RIFX/XFIR</c> signature inside the stream and rewinds the reader to that position.
    /// </summary>
    private void LocateRifx()
    {
        var stream = Context.BaseStream;
        var offset = stream.LocateRifx();
        Context.RegisterRifxOffset(offset);
        Context.Reader.Position = offset;
    }

    private void RegisterCompressions(IEnumerable<BlLegacyCompressionDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            Context.Compressions.Add(descriptor);
        }
    }

    private void RegisterAfterburnerEntries(IEnumerable<BlLegacyResourceEntry> entries)
    {
        foreach (var entry in entries)
        {
            Context.Resources.Add(entry);
        }
    }

    private void RegisterInlineSegments(IReadOnlyDictionary<int, byte[]> segments)
    {
        foreach (var pair in segments)
        {
            Context.Resources.SetInlineSegment(pair.Key, pair.Value);
        }
    }

    private void RegisterClassicEntries(BlBlockMmap map)
    {
        for (var i = 0; i < map.Entries.Count; i++)
        {
            var entryData = map.Entries[i];
            var entry = new BlLegacyResourceEntry(i, entryData.Tag, entryData.Size, entryData.Offset, entryData.Flags, entryData.Attributes, entryData.NextFree);
            Context.Resources.Add(entry);
        }
    }

    private void RegisterKeyRelationships(BlBlockKeyTable keyTable)
    {
        foreach (var entry in keyTable.Entries)
        {
            var link = new BlResourceKeyLink(entry.ChildId, entry.ParentId, entry.Tag);
            Context.Resources.AddRelationship(link);
        }
    }

    /// <summary>
    /// Converts the registered resources into a <see cref="DirFilesContainerDTO"/>, loading payload bytes as needed.
    /// </summary>
    private DirFilesContainerDTO BuildContainer()
    {
        var container = new DirFilesContainerDTO();
        var classicLoader = new BlClassicPayloadLoader(Context);
        var afterburnerLoader = _afterburnerState is null
            ? null
            : new BlAfterburnerPayloadLoader(Context, _afterburnerState);

        foreach (var entry in Context.Resources.Entries)
        {
            if (!ShouldExport(entry))
            {
                continue;
            }

            var payload = entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
                ? afterburnerLoader?.Load(entry) ?? Array.Empty<byte>()
                : classicLoader.Load(entry);

            if (payload.Length == 0)
            {
                continue;
            }

            container.Files.Add(new DirFileResourceDTO
            {
                CastName = string.Empty,
                FileName = BuildFileName(entry),
                Bytes = payload
            });
        }

        return container;
    }

    /// <summary>
    /// Determines whether a resource should be exported based on its tag and declared size.
    /// </summary>
    private static bool ShouldExport(BlLegacyResourceEntry entry)
    {
        if (entry.IsFreeChunk)
        {
            return false;
        }

        return entry.UncompressedSize > 0;
    }

    /// <summary>
    /// Builds the default file name for a resource using its tag and identifier.
    /// </summary>
    private static string BuildFileName(BlLegacyResourceEntry entry)
    {
        return $"{entry.Tag.Value}_{entry.Id:D4}.bin";
    }
}
