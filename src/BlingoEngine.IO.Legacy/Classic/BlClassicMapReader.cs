using System.IO;

using BlingoEngine.IO.Legacy.Classic.Blocks;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic;

/// <summary>
/// Parses classic Director resource maps consisting of <c>imap</c>, <c>mmap</c>, and <c>KEY*</c> chunks. The map records resource
/// metadata as fixed-width rows that point to chunk bodies stored elsewhere in the file.
/// </summary>
internal sealed class BlClassicMapReader
{
    private readonly ReaderContext _context;

    public BlClassicMapReader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Reads the resource map contained within the specified data block, populating the shared resource container.
    /// </summary>
    /// <param name="dataBlock">Chunk metadata describing the payload boundaries.</param>
    public void Read(BlDataBlock dataBlock)
    {
        var reader = _context.Reader;
        reader.Position = dataBlock.PayloadStart;

        while (reader.Position < dataBlock.PayloadEnd)
        {
            var chunkStart = reader.Position;
            var tag = reader.ReadTag();
            var length = reader.ReadUInt32();
            var payloadStart = reader.Position;

            if (tag == BlTag.Imap)
            {
                var imap = BlBlockImap.Read(_context, payloadStart);
                var format = dataBlock.Format;
                format.MapVersion = imap.MapVersion;
                format.ArchiveVersion = imap.ArchiveVersion;
            }
            else if (tag == BlTag.Mmap)
            {
                var mmap = BlBlockMmap.Read(_context, payloadStart);
                RegisterEntries(mmap);
            }
            else if (tag == BlTag.KeyStar)
            {
                ParseKey(payloadStart);
            }

            reader.Position = payloadStart + length;
            reader.AlignToEven();

            if (reader.Position <= chunkStart)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Registers resource entries parsed from the <c>mmap</c> block with the context's resource container.
    /// </summary>
    private void RegisterEntries(BlMmapBlock map)
    {
        var resources = _context.Resources;
        for (var i = 0; i < map.Entries.Count; i++)
        {
            var entryData = map.Entries[i];
            var entry = new BlLegacyResourceEntry(i, entryData.Tag, entryData.Size, entryData.Offset, entryData.Flags, entryData.Attributes, entryData.NextFree);
            resources.Add(entry);
        }
    }

    /// <summary>
    /// Skips the <c>KEY*</c> block that associates parent/child resource IDs. The current implementation records only the map rows.
    /// </summary>
    private void ParseKey(long payloadStart)
    {
        var reader = _context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var entrySize = reader.ReadUInt16();
        var entryCount = reader.ReadUInt32();
        for (var i = 0; i < entryCount; i++)
        {
            reader.Skip(entrySize);
        }

        reader.Position = restore;
    }
}
