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
/// Base class for legacy Director file readers. The derived types share the same parsing pipeline so they simply expose the
/// reader context through this base class.
/// </summary>
public abstract class BlLegacyFileResourceBase : BlLegacyResourceBase
{
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
        var (_, container) = Context.ReadDirFilesContainer();
        return container;
    }
}

/// <summary>
/// Consumes the Director data block and map metadata to produce DTO containers and optional Afterburner state.
/// </summary>
internal sealed class BlDirFilesReader
{
    private readonly ReaderContext _context;

    public BlDirFilesReader(ReaderContext context)
    {
        _context = context;
    }

    public (BlAfterburnerState? State, DirFilesContainerDTO Container) Read()
    {
        _context.ResetRegistries();

        LocateRifx();
        var dataBlock = _context.ReadBlockHeader();

        BlAfterburnerState? afterburnerState = null;

        if (dataBlock.Format.IsAfterburner)
        {
            var afterburner = new BlAfterburnerMapReader(_context);
            var map = afterburner.Read(dataBlock);
            if (!string.IsNullOrEmpty(map.Version))
            {
                dataBlock.Format.AfterburnerVersion = map.Version;
            }

            RegisterCompressions(map.CompressionDescriptors);
            RegisterAfterburnerEntries(map.ResourceEntries);
            RegisterInlineSegments(map.InlineSegments);
            afterburnerState = map.State;
        }
        else
        {
            var classic = new BlClassicMapReader(_context);
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

        var container = BuildContainer(afterburnerState);
        return (afterburnerState, container);
    }

    private void LocateRifx()
    {
        var stream = _context.BaseStream;
        var offset = stream.LocateRifx();
        _context.RegisterRifxOffset(offset);
        _context.Reader.Position = offset;
    }

    private void RegisterCompressions(IEnumerable<BlLegacyCompressionDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            _context.Compressions.Add(descriptor);
        }
    }

    private void RegisterAfterburnerEntries(IEnumerable<BlLegacyResourceEntry> entries)
    {
        foreach (var entry in entries)
        {
            _context.AddResource(entry);
        }
    }

    private void RegisterInlineSegments(IReadOnlyDictionary<int, byte[]> segments)
    {
        foreach (var pair in segments)
        {
            _context.SetResourceInlineSegment(pair.Key, pair.Value);
        }
    }

    private void RegisterClassicEntries(BlBlockMmap map)
    {
        for (var i = 0; i < map.Entries.Count; i++)
        {
            var entryData = map.Entries[i];
            var entry = new BlLegacyResourceEntry(i, entryData.Tag, entryData.Size, entryData.Offset, entryData.Flags, entryData.Attributes, entryData.NextFree);
            _context.AddResource(entry);
        }
    }

    private void RegisterKeyRelationships(BlBlockKeyTable keyTable)
    {
        foreach (var entry in keyTable.Entries)
        {
            var link = new BlResourceKeyLink(entry.ChildId, entry.ParentId, entry.Tag);
            _context.AddResourceRelationship(link);
        }
    }

    private DirFilesContainerDTO BuildContainer(BlAfterburnerState? afterburnerState)
    {
        var container = new DirFilesContainerDTO();
        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (afterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, afterburnerState);
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (!ShouldExport(entry))
            {
                continue;
            }

            byte[] payload = entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
                ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
                : entry.ReadClassicPayload(classicLoader);

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

    private static bool ShouldExport(BlLegacyResourceEntry entry)
    {
        if (entry.IsFreeChunk)
        {
            return false;
        }

        return entry.UncompressedSize > 0;
    }

    private static string BuildFileName(BlLegacyResourceEntry entry)
    {
        return $"{entry.Tag.Value}_{entry.Id:D4}.bin";
    }
}

/// <summary>
/// Exposes the <see cref="BlDirFilesReader"/> through extension methods so the reader context can build DTO containers directly.
/// </summary>
internal static class BlDirFilesReaderExtensions
{
    public static (BlAfterburnerState? State, DirFilesContainerDTO Container) ReadDirFilesContainer(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlDirFilesReader(context);
        return reader.Read();
    }
}
