using System;
using System.IO;

using BlingoEngine.IO.Legacy.Compression;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Afterburner;

/// <summary>
/// Loads Afterburner resource payloads either from the decompressed inline segment table (<c>FGEI</c>) or by seeking into the
/// main movie body relative to the Afterburner data offset. Payloads flagged with a compression index are inflated using the
/// descriptors parsed from the <c>Fcdr</c> block.
/// </summary>
internal sealed class BlAfterburnerPayloadLoader
{
    private readonly ReaderContext _context;
    private readonly BlAfterburnerState _state;

    public BlAfterburnerPayloadLoader(ReaderContext context, BlAfterburnerState state)
    {
        _context = context;
        _state = state;
    }

    /// <summary>
    /// Loads the resource data for the supplied entry, inflating it when a compression descriptor is available.
    /// </summary>
    /// <param name="entry">Resource entry describing the Afterburner offsets and compression index.</param>
    /// <returns>The decoded payload bytes or an empty array when the resource cannot be loaded.</returns>
    public byte[] Load(BlLegacyResourceEntry entry)
    {
        if (entry.UsesInlineData)
        {
            if (_context.Resources.TryGetInlineSegment(entry.Id, out var inline))
            {
                return InflateIfNeeded(entry, inline);
            }

            return Array.Empty<byte>();
        }

        var reader = _context.Reader;
        var restore = reader.Position;
        try
        {
            var offset = _state.BodyOffset + entry.BodyOffset;
            if (offset < 0 || offset > reader.Length)
            {
                return Array.Empty<byte>();
            }

            if (entry.CompressedSize > int.MaxValue)
            {
                return Array.Empty<byte>();
            }

            reader.Position = offset;
            var payload = reader.ReadBytes((int)entry.CompressedSize);
            return InflateIfNeeded(entry, payload);
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

    /// <summary>
    /// Inflates the supplied data using the compression descriptor referenced by the resource entry.
    /// </summary>
    private byte[] InflateIfNeeded(BlLegacyResourceEntry entry, byte[] data)
    {
        if (entry.CompressionIndex < 0)
        {
            return data;
        }

        if (!_context.Compressions.TryGet(entry.CompressionIndex, out var descriptor))
        {
            return data;
        }

        return descriptor.Kind switch
        {
            BlCompressionKind.Zlib => BlZlib.Decompress(data, (int)entry.UncompressedSize),
            _ => data
        };
    }
}
