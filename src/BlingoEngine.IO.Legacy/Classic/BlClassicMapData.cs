using BlingoEngine.IO.Legacy.Classic.Blocks;

namespace BlingoEngine.IO.Legacy.Classic;

/// <summary>
/// Aggregates the control blocks decoded from the classic <c>imap</c>, <c>mmap</c>, and <c>KEY*</c> chunks.
/// </summary>
internal sealed class BlClassicMapData
{
    public BlClassicMapData(BlBlockImap? imap, BlBlockMmap? mmap, BlBlockKeyTable? keyTable)
    {
        Imap = imap;
        Mmap = mmap;
        KeyTable = keyTable;
    }

    public BlBlockImap? Imap { get; }

    public BlBlockMmap? Mmap { get; }

    public BlBlockKeyTable? KeyTable { get; }
}
