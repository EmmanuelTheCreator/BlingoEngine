using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Classic.Blocks;
using BlingoEngine.IO.Legacy.Compression;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;



/// <summary>
/// Exposes the <see cref="BlDirFilesReader"/> through extension methods so the reader context can build DTO containers directly.
/// </summary>
internal static class BlDirFilesReaderExtensions
{
    public static DirFilesContainerDTO ReadDirFilesContainer(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlDirFilesReader(context);
        return reader.Read();
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

    public DirFilesContainerDTO Read()
    {
        _context.ResetRegistries();

        LocateRifx();
        var dataBlock = _context.ReadBlockHeader();

        if (dataBlock.Format.IsAfterburner)
        {
            var map = _context.ReadAfterburnerMap(dataBlock);
            if (!string.IsNullOrEmpty(map.Version))
                dataBlock.Format.AfterburnerVersion = map.Version;

            RegisterCompressions(map.CompressionDescriptors);
            RegisterAfterburnerEntries(map.ResourceEntries);
            RegisterInlineSegments(map.InlineSegments);
        }
        else
        {
            var map = dataBlock.ReadClassicMapData(_context);

            if (map.Imap is not null)
            {
                dataBlock.Format.MapVersion = map.Imap.MapVersion;
                dataBlock.Format.ArchiveVersion = map.Imap.ArchiveVersion;
            }

            if (map.Mmap is not null)
                RegisterClassicEntries(map.Mmap);

            if (map.KeyTable is not null)
                RegisterKeyRelationships(map.KeyTable);
        }

        return BuildContainer();
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
            _context.AddCompressionDescriptor(descriptor);
    }

    private void RegisterAfterburnerEntries(IEnumerable<BlLegacyResourceEntry> entries)
    {
        foreach (var entry in entries)
            _context.AddResource(entry);
    }

    private void RegisterInlineSegments(IReadOnlyDictionary<int, byte[]> segments)
    {
        foreach (var pair in segments)
            _context.SetResourceInlineSegment(pair.Key, pair.Value);
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

    private DirFilesContainerDTO BuildContainer()
    {
        var container = new DirFilesContainerDTO();
        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (!ShouldExport(entry))
                continue;

            byte[] payload = entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
                ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
                : entry.ReadClassicPayload(classicLoader);

            if (payload.Length == 0)
                continue;

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
            return false;

        return entry.UncompressedSize > 0;
    }

    private static string BuildFileName(BlLegacyResourceEntry entry)
    {
        return $"{entry.Tag.Value}_{entry.Id:D4}.bin";
    }
}
