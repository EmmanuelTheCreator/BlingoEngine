using System.Collections.Generic;

namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Aggregates the information extracted from an <c>mmap</c> block: the table row size, the number of populated entries, and
/// the parsed rows themselves. Director stores the entry size as a 16-bit value immediately after the <c>mmap</c> tag and uses
/// it to pad rows with implementation-specific bytes.
/// </summary>
public sealed class BlMmapBlock
{
    /// <summary>
    /// Gets or sets the byte length of each entry as recorded in the <c>mmap</c> header.
    /// </summary>
    public ushort EntrySize { get; set; }

    /// <summary>
    /// Gets or sets the number of populated rows described by the block.
    /// </summary>
    public uint EntryCount { get; set; }

    /// <summary>
    /// Gets the collection of resource entries decoded from the block.
    /// </summary>
    public List<BlMmapEntry> Entries { get; } = new();
}
