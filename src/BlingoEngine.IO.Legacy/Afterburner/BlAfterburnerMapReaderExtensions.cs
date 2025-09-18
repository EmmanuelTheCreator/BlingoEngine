using System;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Afterburner;

/// <summary>
/// Extension helpers that expose the <see cref="BlAfterburnerMapReader"/> through the shared <see cref="ReaderContext"/> and
/// <see cref="BlDataBlock"/> types used by the legacy pipeline.
/// </summary>
internal static class BlAfterburnerMapReaderExtensions
{
    public static BlAfterburnerMapData ReadAfterburner(this ReaderContext context, BlDataBlock dataBlock)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dataBlock);

        var reader = new BlAfterburnerMapReader(context);
        var map = reader.Read(dataBlock);
        context.SetAfterburnerState(map.State);
        return map;
    }

    public static BlAfterburnerMapData ReadAfterburner(this BlDataBlock dataBlock, ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.ReadAfterburner(dataBlock);
    }

    public static BlAfterburnerMapData ReadAfterburnerMap(this ReaderContext context, BlDataBlock dataBlock)
    {
        return context.ReadAfterburner(dataBlock);
    }

    public static BlAfterburnerMapData ReadAfterburnerMap(this BlDataBlock dataBlock, ReaderContext context)
    {
        return context.ReadAfterburner(dataBlock);
    }
}
