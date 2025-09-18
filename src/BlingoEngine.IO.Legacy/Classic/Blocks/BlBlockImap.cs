using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Provides helpers for reading the <c>imap</c> control block. The payload begins immediately after the chunk header and contains
/// four unsigned 32-bit values: block length, map version, <c>mmap</c> offset, and archive version.
/// </summary>
internal static class BlBlockImap
{
    /// <summary>
    /// Reads the four 32-bit values stored in the <c>imap</c> payload.
    /// </summary>
    /// <param name="context">Reader context positioned on the movie bytes.</param>
    /// <param name="payloadStart">Absolute offset where the <c>imap</c> data begins.</param>
    /// <returns>The decoded block values used by subsequent map parsing.</returns>
    public static BlImapBlock Read(ReaderContext context, long payloadStart)
    {
        var reader = context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var length = reader.ReadUInt32();
        var mapVersion = reader.ReadUInt32();
        var mmapOffset = reader.ReadUInt32();
        var archiveVersion = reader.ReadUInt32();

        reader.Position = restore;
        return new BlImapBlock(length, mapVersion, mmapOffset, archiveVersion);
    }
}
