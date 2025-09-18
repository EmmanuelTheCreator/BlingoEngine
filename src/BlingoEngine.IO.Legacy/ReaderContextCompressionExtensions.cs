using System;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Provides helper methods for registering compression descriptors with a <see cref="ReaderContext"/> without exposing the
/// underlying container.
/// </summary>
internal static class ReaderContextCompressionExtensions
{
    public static void AddCompression(this ReaderContext context, BlLegacyCompressionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(descriptor);

        context.Compressions.Add(descriptor);
    }
}
