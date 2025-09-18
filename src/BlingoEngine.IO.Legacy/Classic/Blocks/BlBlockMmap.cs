using System.Collections.Generic;
using System.Text;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Represents the rows stored in an <c>mmap</c> block.
/// </summary>
internal sealed class BlBlockMmap
{
    public ushort EntrySize { get; init; }
    public uint EntryCount { get; init; }
    public List<BlBlockMmapEntry> Entries { get; } = new();

    public string ToMarkDown()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Entry Size: {EntrySize}");
        builder.AppendLine($"Entry Count: {EntryCount}");
        builder.AppendLine();
        builder.AppendLine("| Index | Tag | Size | Offset | Flags | Attributes | Next Free |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

        for (var i = 0; i < Entries.Count; i++)
        {
            var entry = Entries[i];
            builder.Append("| ")
                .Append(i)
                .Append(" | ")
                .Append(entry.Tag.Value)
                .Append(" | ")
                .Append(entry.Size)
                .Append(" | ")
                .Append(entry.Offset)
                .Append(" | ")
                .Append(entry.Flags)
                .Append(" | ")
                .Append(entry.Attributes)
                .Append(" | ")
                .Append(entry.NextFree)
                .AppendLine(" |");
        }

        return builder.ToString();
    }
}

/// <summary>
/// Represents a single resource row from the <c>mmap</c> table.
/// </summary>
internal readonly record struct BlBlockMmapEntry(BlTag Tag, uint Size, uint Offset, ushort Flags, ushort Attributes, uint NextFree);

/// <summary>
/// Provides helpers for reading the <c>mmap</c> control block that indexes classic resources.
/// </summary>
internal static class BlBlockMmapExtensions
{
    /// <summary>
    /// Reads the repeated entry rows contained inside the <c>mmap</c> payload.
    /// </summary>
    /// <param name="context">Reader context positioned on the movie bytes.</param>
    /// <param name="payloadStart">Absolute offset where the <c>mmap</c> data begins.</param>
    /// <returns>A block containing the parsed table rows.</returns>
    public static BlBlockMmap ReadMmap(this ReaderContext context, long payloadStart)
    {
        var reader = context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var block = new BlBlockMmap
        {
            EntrySize = reader.ReadUInt16(),
            EntryCount = reader.ReadUInt32()
        };

        for (uint i = 0; i < block.EntryCount; i++)
        {
            var entryStart = reader.Position;
            var tag = reader.ReadTag();
            var size = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var flags = reader.ReadUInt16();
            var attributes = reader.ReadUInt16();
            var nextFree = reader.ReadUInt32();

            block.Entries.Add(new BlBlockMmapEntry(tag, size, offset, flags, attributes, nextFree));

            var consumed = reader.Position - entryStart;
            var padding = (long)block.EntrySize - consumed;
            if (padding > 0)
            {
                reader.Skip(padding);
            }
        }

        reader.Position = restore;
        return block;
    }
}
