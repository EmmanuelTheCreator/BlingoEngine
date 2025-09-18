namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents a single row of the <c>mmap</c> table. Each entry consumes the 12 bytes that follow the resource tag:
/// four bytes for the stored length, four bytes for the map offset, two bytes for flags, two bytes for attributes,
/// and four bytes for the next-free pointer used by Director's allocator.
/// </summary>
/// <param name="Tag">Resource type stored as a four-character code.</param>
/// <param name="Size">Declared chunk size excluding the chunk header.</param>
/// <param name="Offset">Absolute file offset pointing to the chunk header.</param>
/// <param name="Flags">16-bit flags recorded by the archive.</param>
/// <param name="Attributes">Unknown 16-bit field preserved for parity with observed dumps.</param>
/// <param name="NextFree">Next-free pointer chaining unused entries.</param>
public readonly record struct BlMmapEntry(BlTag Tag, uint Size, uint Offset, ushort Flags, ushort Attributes, uint NextFree);
