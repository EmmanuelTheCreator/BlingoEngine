using System;

namespace BlingoEngine.IO.Legacy.Scripts;

/// <summary>
/// Represents the compiled Lingo payload stored inside an <c>Lscr</c> resource.
/// The loader preserves the raw bytes so higher layers can decompile handlers or
/// rebuild property tables without re-reading the Director container.
/// </summary>
internal sealed class BlLegacyScript
{
    /// <summary>
    /// Gets the resource identifier associated with the <c>Lscr</c> entry.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the script category resolved from the <c>CASt</c> selector word.
    /// </summary>
    public BlLegacyScriptFormatKind Format { get; }

    /// <summary>
    /// Gets the compiled <c>Lscr</c> payload exactly as exported by Director. The
    /// buffer retains the header fields (duplicate length words, script and parent
    /// identifiers, and flag values) followed by the offset tables for handler
    /// vectors, property/global name lists, literal pools, and the bytecode stream.
    /// This mirrors the layout documented in the project notes so tooling can walk
    /// the individual sections without depending on the original map.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyScript"/> class.
    /// </summary>
    public BlLegacyScript(int resourceId, BlLegacyScriptFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
