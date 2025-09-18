using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Represents the metadata stored inside the <c>imap</c> control block.
/// </summary>
internal sealed class BlBlockImap
{
    public uint Length { get; init; }
    public uint MapVersion { get; init; }
    public uint MapOffset { get; init; }
    public uint ArchiveVersion { get; init; }
}

/// <summary>
/// Provides helper methods for reading <see cref="BlBlockImap"/> structures from the movie stream.
/// </summary>
internal static class BlBlockImapExtensions
{
    /// <summary>
    /// Reads the four 32-bit values stored in the <c>imap</c> payload.
    /// </summary>
    /// <param name="context">Reader context positioned on the movie bytes.</param>
    /// <param name="payloadStart">Absolute offset where the <c>imap</c> data begins.</param>
    /// <returns>The decoded block values used by subsequent map parsing.</returns>
    public static BlBlockImap ReadImap(this ReaderContext context, long payloadStart)
    {
        var reader = context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var block = new BlBlockImap
        {
            Length = reader.ReadUInt32(),
            MapVersion = reader.ReadUInt32(),
            MapOffset = reader.ReadUInt32(),
            ArchiveVersion = reader.ReadUInt32()
        };

        reader.Position = restore;
        return block;
    }
}
