using BlingoEngine.IO.Legacy.Classic.Blocks;
using BlingoEngine.IO.Legacy.Core;
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
    /// Reads the resource map contained within the specified data block and returns the decoded structures.
    /// </summary>
    /// <param name="dataBlock">Chunk metadata describing the payload boundaries.</param>
    public BlClassicMapData Read(BlDataBlock dataBlock)
    {
        var reader = _context.Reader;
        reader.Position = dataBlock.PayloadStart;

        BlBlockImap? imap = null;
        BlBlockMmap? mmap = null;
        BlBlockKeyTable? keyTable = null;

        while (reader.Position < dataBlock.PayloadEnd)
        {
            if (dataBlock.PayloadEnd - reader.Position < 8)
                break;

            var chunkStart = reader.Position;
            var tag = reader.ReadTag();
            var length = reader.ReadUInt32();
            var payloadStart = reader.Position;

            if (tag == BlTag.Imap)
                imap = _context.ReadImap(payloadStart);
            else if (tag == BlTag.Mmap)
                mmap = _context.ReadMmap(payloadStart);
            else if (tag == BlTag.KeyStar)
                keyTable = _context.ReadKeyTable(payloadStart);

            reader.Position = payloadStart + length;
            reader.AlignToEven();

            if (reader.Position <= chunkStart)
                break;
        }

        return new BlClassicMapData(imap, mmap, keyTable);
    }
}
