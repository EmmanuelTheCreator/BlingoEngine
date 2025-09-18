using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Compression;

/// <summary>
/// Represents an entry decoded from the <c>Fcdr</c> block. Each entry is identified by a table index, a 16-byte identifier, and
/// an ASCII descriptor name that hint at the compression scheme applied to Afterburner payloads.
/// </summary>


public sealed class BlLegacyCompressionDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyCompressionDescriptor"/> class.
    /// </summary>
    public BlLegacyCompressionDescriptor(int index, ReadOnlyMemory<byte> identifier, string name, BlCompressionKind kind)
    {
        Index = index;
        Identifier = identifier;
        Name = name;
        Kind = kind;
    }

    /// <summary>
    /// Gets the zero-based index assigned to this descriptor inside the <c>Fcdr</c> table.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the 16-byte identifier read directly from the <c>Fcdr</c> payload.
    /// </summary>
    public ReadOnlyMemory<byte> Identifier { get; }

    /// <summary>
    /// Gets the ASCII label stored after the identifier bytes.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the resolved compression kind for this descriptor.
    /// </summary>
    public BlCompressionKind Kind { get; }
}

