using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Provides helpers for reading the <c>mmap</c> control block that indexes classic resources. The payload begins with a
/// 16-bit entry size and a 32-bit entry count followed by repeated rows containing a tag, size, offset, flag fields, and the
/// next-free pointer Director uses for allocation bookkeeping.
/// </summary>
internal static class BlBlockMmap
{
    /// <summary>
    /// Reads the repeated entry rows contained inside the <c>mmap</c> payload.
    /// </summary>
    /// <param name="context">Reader context positioned on the movie bytes.</param>
    /// <param name="payloadStart">Absolute offset where the <c>mmap</c> data begins.</param>
    /// <returns>A block containing the parsed table rows.</returns>
    public static BlMmapBlock Read(ReaderContext context, long payloadStart)
    {
        var reader = context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var block = new BlMmapBlock
        {
            EntrySize = reader.ReadUInt16(),
            EntryCount = reader.ReadUInt32()
        };

        for (uint i = 0; i < block.EntryCount; i++)
        {
            var entryStart = reader.Position;
            var tag = reader.ReadTag();
            var size = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var flags = reader.ReadUInt16();
            var attributes = reader.ReadUInt16();
            var nextFree = reader.ReadUInt32();

            block.Entries.Add(new BlMmapEntry(tag, size, offset, flags, attributes, nextFree));

            var consumed = reader.Position - entryStart;
            var padding = (long)block.EntrySize - consumed;
            if (padding > 0)
            {
                reader.Skip(padding);
            }
        }

        reader.Position = restore;
        return block;
    }
}
