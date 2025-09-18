using System;
using System.IO;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic;

/// <summary>
/// Loads classic resource payloads by seeking to the 8-byte chunk headers pointed to by the <c>mmap</c> table. Each payload is
/// validated by comparing the stored four-character tag before reading the declared length.
/// </summary>
internal sealed class BlClassicPayloadLoader
{
    private readonly ReaderContext _context;

    public BlClassicPayloadLoader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads the raw chunk bytes for the supplied map entry.
    /// </summary>
    /// <param name="entry">Resource map entry describing the chunk location.</param>
    /// <returns>The raw chunk bytes or an empty array when the payload cannot be resolved.</returns>
    public byte[] Load(BlLegacyResourceEntry entry)
    {
        var reader = _context.Reader;
        var offsets = new[] { entry.MapOffset, _context.RifxOffset + entry.MapOffset };

        foreach (var candidate in offsets)
        {
            if (candidate < 0 || candidate > reader.Length - 8)
            {
                continue;
            }

            var restore = reader.Position;
            try
            {
                reader.Position = candidate;
                var chunkTag = reader.ReadTag();
                if (chunkTag != entry.Tag)
                {
                    continue;
                }

                var length = reader.ReadUInt32();
                if (length == 0)
                {
                    return Array.Empty<byte>();
                }

                if (length > int.MaxValue)
                {
                    continue;
                }

                return reader.ReadBytes((int)length);
            }
            catch (EndOfStreamException)
            {
                return Array.Empty<byte>();
            }
            finally
            {
                reader.Position = restore;
            }
        }

        return Array.Empty<byte>();
    }
}
