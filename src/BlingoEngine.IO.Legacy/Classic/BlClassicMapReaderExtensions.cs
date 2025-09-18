using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic;

/// <summary>
/// Extension helpers for wiring the <see cref="BlClassicMapReader"/> into the shared context and block structures.
/// </summary>
internal static class BlClassicMapReaderExtensions
{
    public static BlClassicMapData ReadClassicMapData(this ReaderContext context, BlDataBlock dataBlock)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dataBlock);

        var reader = new BlClassicMapReader(context);
        return reader.Read(dataBlock);
    }

    public static BlClassicMapData ReadClassicMapData(this BlDataBlock dataBlock, ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.ReadClassicMapData(dataBlock);
    }

    public static BlClassicMapData ReadMapData(this ReaderContext context, BlDataBlock dataBlock) => context.ReadClassicMapData(dataBlock);

    public static BlClassicMapData ReadMapData(this BlDataBlock dataBlock, ReaderContext context) => context.ReadClassicMapData(dataBlock);
}
