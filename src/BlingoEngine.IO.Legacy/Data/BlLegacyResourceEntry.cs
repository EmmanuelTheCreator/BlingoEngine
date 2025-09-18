namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents a single resource entry decoded from either the <c>mmap</c> table or the Afterburner <c>ABMP</c> stream.
/// Classic entries expose their chunk offset through <see cref="MapOffset"/>, while Afterburner entries express their storage
/// via <see cref="BodyOffset"/>, <see cref="CompressedSize"/>, and <see cref="CompressionIndex"/>.
/// </summary>
public sealed class BlLegacyResourceEntry
{
    /// <summary>
    /// Initializes a new <see cref="BlLegacyResourceEntry"/> describing a classic memory-map row.
    /// </summary>
    public BlLegacyResourceEntry(int id, BlTag tag, uint size, uint mapOffset, ushort flags, ushort attributes, uint nextFree)
    {
        Id = id;
        Tag = tag;
        MapOffset = mapOffset;
        DeclaredSize = size;
        Flags = flags;
        Attributes = attributes;
        NextFree = nextFree;
        StorageKind = BlResourceStorageKind.ClassicChunk;
        BodyOffset = 0;
        CompressedSize = size;
        UncompressedSize = size;
        CompressionIndex = -1;
    }

    /// <summary>
    /// Initializes a new <see cref="BlLegacyResourceEntry"/> describing an Afterburner map row.
    /// </summary>
    public BlLegacyResourceEntry(int id, BlTag tag, int bodyOffset, uint compressedSize, uint uncompressedSize, int compressionIndex)
    {
        Id = id;
        Tag = tag;
        MapOffset = bodyOffset;
        DeclaredSize = uncompressedSize;
        Flags = 0;
        Attributes = 0;
        NextFree = 0;
        StorageKind = BlResourceStorageKind.AfterburnerSegment;
        BodyOffset = bodyOffset;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;
        CompressionIndex = compressionIndex;
    }

    /// <summary>
    /// Gets the table index assigned to the resource.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the resource type tag (four-character code).
    /// </summary>
    public BlTag Tag { get; }

    /// <summary>
    /// Gets the chunk offset recorded in the <c>mmap</c> table. For Afterburner entries this mirrors the <see cref="BodyOffset"/>.
    /// </summary>
    public long MapOffset { get; }

    /// <summary>
    /// Gets the declared chunk size as stored in the map entry.
    /// </summary>
    public uint DeclaredSize { get; }

    /// <summary>
    /// Gets the 16-bit flag field copied from the <c>mmap</c> row.
    /// </summary>
    public ushort Flags { get; }

    /// <summary>
    /// Gets the additional 16-bit attribute field preserved for parity with original files.
    /// </summary>
    public ushort Attributes { get; }

    /// <summary>
    /// Gets the pointer that chains free entries inside the map.
    /// </summary>
    public uint NextFree { get; }

    /// <summary>
    /// Gets the storage kind describing whether the entry comes from a classic map or an Afterburner table.
    /// </summary>
    public BlResourceStorageKind StorageKind { get; }

    /// <summary>
    /// Gets the relative offset used to locate the resource body. Afterburner entries use <c>-1</c> to mark inline segments.
    /// </summary>
    public int BodyOffset { get; }

    /// <summary>
    /// Gets the stored payload length. Classic entries match <see cref="DeclaredSize"/> while Afterburner entries may be compressed.
    /// </summary>
    public uint CompressedSize { get; }

    /// <summary>
    /// Gets the expected length after decompression.
    /// </summary>
    public uint UncompressedSize { get; }

    /// <summary>
    /// Gets the compression descriptor index referenced by Afterburner entries. Classic entries set this to <c>-1</c>.
    /// </summary>
    public int CompressionIndex { get; }

    /// <summary>
    /// Gets a value indicating whether the entry corresponds to a free or junk chunk.
    /// </summary>
    public bool IsFreeChunk => Tag == BlTag.Free || Tag == BlTag.Junk;

    /// <summary>
    /// Gets a value indicating whether the entry is stored inside the initial load segment buffer.
    /// </summary>
    public bool UsesInlineData => StorageKind == BlResourceStorageKind.AfterburnerSegment && BodyOffset < 0;
}
