using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Afterburner;

/// <summary>
/// Aggregates the data decoded from Afterburner control chunks.
/// </summary>
internal sealed class BlAfterburnerMapData
{
    public BlAfterburnerMapData(
        string? version,
        IReadOnlyList<BlLegacyCompressionDescriptor> compressionDescriptors,
        IReadOnlyList<BlLegacyResourceEntry> resourceEntries,
        IReadOnlyDictionary<int, byte[]> inlineSegments,
        BlAfterburnerState state)
    {
        Version = version;
        CompressionDescriptors = compressionDescriptors;
        ResourceEntries = resourceEntries;
        InlineSegments = inlineSegments;
        State = state;
    }

    public string? Version { get; }

    public IReadOnlyList<BlLegacyCompressionDescriptor> CompressionDescriptors { get; }

    public IReadOnlyList<BlLegacyResourceEntry> ResourceEntries { get; }

    public IReadOnlyDictionary<int, byte[]> InlineSegments { get; }

    public BlAfterburnerState State { get; }
}
