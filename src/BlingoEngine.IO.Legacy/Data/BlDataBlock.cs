namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents the top-level movie chunk boundaries. After the 12-byte header, the payload begins at
/// byte offset 0x0C and spans <see cref="DeclaredSize"/> bytes, ending at <see cref="PayloadEnd"/>.
/// The <see cref="Format"/> property carries the codec and version metadata decoded from adjacent bytes.
/// </summary>
public sealed class BlDataBlock
{
    /// <summary>
    /// Gets or sets the declared payload size written at bytes 0x04-0x07 of the movie header.
    /// </summary>
    public uint DeclaredSize { get; set; }

    /// <summary>
    /// Gets or sets the absolute stream position where the chunk payload begins.
    /// </summary>
    public long PayloadStart { get; set; }

    /// <summary>
    /// Gets or sets the absolute stream position that marks the first byte past the chunk payload.
    /// </summary>
    public long PayloadEnd { get; set; }

    /// <summary>
    /// Gets the format description associated with the chunk.
    /// </summary>
    public BlDataFormat Format { get; } = new();
}
