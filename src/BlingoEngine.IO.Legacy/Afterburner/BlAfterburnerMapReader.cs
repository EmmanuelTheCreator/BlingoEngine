using BlingoEngine.IO.Legacy.Compression;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Afterburner;

/// <summary>
/// Reads the Afterburner metadata stream composed of <c>Fver</c>, <c>Fcdr</c>, <c>ABMP</c>, and <c>FGEI</c> chunks. Each block
/// uses variable-length integers to describe resource identifiers, offsets, and compression descriptors before the payloads are
/// optionally inflated from zlib-compressed byte arrays.
/// </summary>
internal sealed class BlAfterburnerMapReader
{
    private readonly ReaderContext _context;

    public BlAfterburnerMapReader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Reads the Afterburner control chunks from the payload defined by the <see cref="BlDataBlock"/> header.
    /// </summary>
    /// <param name="dataBlock">Top-level chunk boundaries decoded from the 12-byte header.</param>
    /// <returns>State describing the Afterburner body offsets used by payload lookups.</returns>
    public BlAfterburnerMapData Read(BlDataBlock dataBlock)
    {
        var reader = _context.Reader;
        reader.Position = dataBlock.PayloadStart;

        var fver = ReadChunkHeader(reader);
        EnsureChunkTag(fver, BlTag.Fver);
        var version = ReadFver(fver);

        var fcdr = ReadChunkHeader(reader);
        EnsureChunkTag(fcdr, BlTag.Fcdr);
        var descriptors = ReadFcdr(fcdr);

        var abmp = ReadChunkHeader(reader);
        EnsureChunkTag(abmp, BlTag.Abmp);
        var entries = ReadAbmp(abmp);

        var fgei = ReadChunkHeader(reader);
        EnsureChunkTag(fgei, BlTag.Fgei);
        var inlineData = ReadFgei(fgei, entries);

        return new BlAfterburnerMapData(
            version,
            descriptors,
            entries,
            inlineData.Segments,
            inlineData.State);
    }

    /// <summary>
    /// Reads a chunk header consisting of a four-byte tag followed by a variable-length payload size.
    /// </summary>
    private static ChunkHeader ReadChunkHeader(BlStreamReader reader)
    {
        var tag = reader.ReadTag();
        var length = reader.ReadVariableUInt32();
        var start = reader.Position;
        return new ChunkHeader(tag, length, start);
    }

    /// <summary>
    /// Validates that the chunk tag matches the expected Afterburner block identifier.
    /// </summary>
    private static void EnsureChunkTag(ChunkHeader header, BlTag expected)
    {
        if (header.Tag != expected)
        {
            throw new InvalidDataException($"Expected chunk '{expected}' but found '{header.Tag}'.");
        }
    }

    /// <summary>
    /// Reads the <c>Fver</c> block, capturing the variable-length Afterburner build string stored after the version numbers.
    /// </summary>
    private string? ReadFver(ChunkHeader header)
    {
        var reader = _context.Reader;
        reader.Position = header.Start;

        var fverVersion = reader.ReadVariableUInt32();
        if (fverVersion >= 0x401)
        {
            reader.ReadVariableUInt32();
            reader.ReadVariableUInt32();
        }

        string? version = null;
        if (fverVersion >= 0x501)
        {
            var length = reader.ReadByte();
            version = reader.ReadAsciiString(length);
        }

        var consumed = reader.Position - header.Start;
        var remaining = header.Length - consumed;
        if (remaining > 0)
        {
            reader.Skip(remaining);
        }

        reader.Position = header.Start + header.Length;
        return version;
    }

    /// <summary>
    /// Reads the <c>Fcdr</c> chunk containing the compressed descriptor table. The chunk stores a count, followed by 16-byte
    /// identifiers and trailing ASCII names for each compression format.
    /// </summary>
    private IReadOnlyList<BlLegacyCompressionDescriptor> ReadFcdr(ChunkHeader header)
    {
        var reader = _context.Reader;
        reader.Position = header.Start;

        if (header.Length > int.MaxValue)
        {
            throw new InvalidDataException("Fcdr chunk is larger than supported by the reader.");
        }

        var compressed = reader.ReadBytes((int)header.Length);
        var data = BlZlib.Decompress(compressed);
        reader.Position = header.Start + header.Length;

        var descriptorReader = _context.CreateMemoryReader(data);
        var count = descriptorReader.ReadUInt16();
        var identifiers = new List<byte[]>(count);
        var descriptors = new List<BlLegacyCompressionDescriptor>(count);
        for (var i = 0; i < count; i++)
        {
            var buffer = descriptorReader.ReadBytes(16);
            identifiers.Add(buffer);
        }

        for (var i = 0; i < count; i++)
        {
            var name = descriptorReader.ReadCString();
            var identifier = identifiers[i];
            var kind = BlCompressionFormat.Resolve(identifier, name);
            var descriptor = new BlLegacyCompressionDescriptor(i, identifier, name, kind);
            descriptors.Add(descriptor);
        }

        return descriptors;
    }

    /// <summary>
    /// Reads the <c>ABMP</c> chunk that lists all resource entries. Each entry stores the resource ID, offset, compressed and
    /// uncompressed sizes, compression table index, and a four-character tag using variable-length integers.
    /// </summary>
    private IReadOnlyList<BlLegacyResourceEntry> ReadAbmp(ChunkHeader header)
    {
        var reader = _context.Reader;
        reader.Position = header.Start;

        if (header.Length > int.MaxValue)
        {
            throw new InvalidDataException("ABMP chunk is larger than supported by the reader.");
        }

        var compressionMode = reader.ReadVariableUInt32();
        var expectedSize = reader.ReadVariableUInt32();
        var remaining = (int)(header.Start + header.Length - reader.Position);
        var compressed = reader.ReadBytes(remaining);
        var payload = compressionMode == 0
            ? compressed
            : BlZlib.Decompress(compressed, (int)expectedSize);

        reader.Position = header.Start + header.Length;

        var mapReader = _context.CreateMemoryReader(payload);
        var entries = new List<BlLegacyResourceEntry>();
        mapReader.ReadVariableUInt32();
        mapReader.ReadVariableUInt32();
        var resourceCount = mapReader.ReadVariableUInt32();
        for (var i = 0; i < resourceCount; i++)
        {
            var resourceId = unchecked((int)mapReader.ReadVariableUInt32());
            var offset = unchecked((int)mapReader.ReadVariableUInt32());
            var compressedSize = mapReader.ReadVariableUInt32();
            var uncompressedSize = mapReader.ReadVariableUInt32();
            var compressionIndex = unchecked((int)mapReader.ReadVariableUInt32());
            var tag = mapReader.ReadTag();

            var entry = new BlLegacyResourceEntry(resourceId, tag, offset, compressedSize, uncompressedSize, compressionIndex);
            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Reads the <c>FGEI</c> chunk holding the initial load segment bytes for resources with an offset of <c>-1</c>.
    /// </summary>
    private (BlAfterburnerState State, IReadOnlyDictionary<int, byte[]> Segments) ReadFgei(ChunkHeader header, IReadOnlyList<BlLegacyResourceEntry> entries)
    {
        var reader = _context.Reader;
        var bodyOffset = header.Start;

        var state = new BlAfterburnerState(bodyOffset);
        var entryById = new Dictionary<int, BlLegacyResourceEntry>(entries.Count);
        foreach (var entry in entries)
        {
            entryById[entry.Id] = entry;
        }

        if (!entryById.TryGetValue(2, out var inlineEntry))
        {
            throw new InvalidDataException("Afterburner map is missing inline segment resource (id 2).");
        }

        var segments = new Dictionary<int, byte[]>();
        if (inlineEntry.CompressedSize > 0)
        {
            reader.Position = header.Start;
            if (inlineEntry.CompressedSize > int.MaxValue)
            {
                throw new InvalidDataException("Inline segment is larger than supported by the reader.");
            }

            var compressed = reader.ReadBytes((int)inlineEntry.CompressedSize);
            var inlineData = BlZlib.Decompress(compressed, (int)inlineEntry.UncompressedSize);
            var parsed = ParseInlineSegments(inlineData, entryById);
            foreach (var pair in parsed)
            {
                segments[pair.Key] = pair.Value;
            }
        }
        else
        {
            segments[inlineEntry.Id] = Array.Empty<byte>();
        }

        reader.Position = header.Start + header.Length;
        return (state, segments);
    }

    /// <summary>
    /// Parses the inline segment table that stores raw bytes for resources referenced by negative offsets.
    /// </summary>
    private Dictionary<int, byte[]> ParseInlineSegments(byte[] data, Dictionary<int, BlLegacyResourceEntry> entries)
    {
        var reader = _context.CreateMemoryReader(data);
        var segments = new Dictionary<int, byte[]>();
        while (reader.Position < reader.Length)
        {
            var resourceId = unchecked((int)reader.ReadVariableUInt32());
            if (!entries.TryGetValue(resourceId, out var entry))
            {
                break;
            }

            if (entry.CompressedSize > int.MaxValue)
            {
                break;
            }

            var length = (int)entry.CompressedSize;
            if (length == 0)
            {
                segments[resourceId] = Array.Empty<byte>();
                continue;
            }

            if (reader.Position + length > reader.Length)
            {
                break;
            }

            var payload = reader.ReadBytes(length);
            segments[resourceId] = payload;
        }

        return segments;
    }

    private readonly record struct ChunkHeader(BlTag Tag, uint Length, long Start);
}
