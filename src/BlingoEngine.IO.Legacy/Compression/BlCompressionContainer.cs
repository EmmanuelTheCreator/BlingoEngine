using BlingoEngine.IO.Legacy.Core;
using System.Diagnostics.CodeAnalysis;

namespace BlingoEngine.IO.Legacy.Compression;


public static class BlLegacyCompressionExensions
{
    /// <summary>
    /// Provides helper methods for registering compression descriptors with a <see cref="ReaderContext"/> without exposing the
    /// underlying container.
    /// </summary>
    public static void AddCompressionDescriptor(this ReaderContext context, BlLegacyCompressionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(descriptor);

        context.Compressions.Add(descriptor);
    }

}


/// <summary>
/// Tracks compression descriptors declared in the <c>Fcdr</c> block. Each descriptor is introduced by a 16-byte identifier
/// followed by an ASCII label, matching the byte layout described for compression catalogs: the block lists the number of
/// entries, then stores the raw 16-byte identifiers, and finally the null-terminated names. This container allows lookups by
/// the order index used inside <c>ABMP</c> resource entries.
/// </summary>
public sealed class BlCompressionContainer
{
    private readonly Dictionary<int, BlLegacyCompressionDescriptor> _byIndex = new();

    /// <summary>
    /// Removes all registered compression descriptors so that each movie starts with a clean table.
    /// </summary>
    public void Reset() => _byIndex.Clear();

    /// <summary>
    /// Adds a descriptor decoded from the <c>Fcdr</c> bytes to the container.
    /// </summary>
    /// <param name="descriptor">The descriptor built from the 16-byte identifier and trailing name.</param>
    public void Add(BlLegacyCompressionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _byIndex[descriptor.Index] = descriptor;
    }

    /// <summary>
    /// Attempts to resolve a descriptor using the index stored in <c>ABMP</c> entries.
    /// </summary>
    /// <param name="index">The compression table index referenced by a resource.</param>
    /// <param name="descriptor">When this method returns, contains the descriptor associated with <paramref name="index"/>.</param>
    /// <returns><c>true</c> when a descriptor is registered for the supplied index; otherwise, <c>false</c>.</returns>
    public bool TryGet(int index, [NotNullWhen(true)] out BlLegacyCompressionDescriptor? descriptor)
    {
        if (_byIndex.TryGetValue(index, out var value))
        {
            descriptor = value;
            return true;
        }

        descriptor = null;
        return false;
    }
}
