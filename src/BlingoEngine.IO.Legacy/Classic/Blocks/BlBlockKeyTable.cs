using System.Collections.Generic;
using System.IO;
using System.Text;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Represents the rows stored inside a <c>KEY*</c> block.
/// </summary>
internal sealed class BlBlockKeyTable
{
    public ushort EntrySize { get; init; }
    public ushort SecondaryEntrySize { get; init; }
    public uint TotalEntryCount { get; init; }
    public uint UsedEntryCount { get; init; }
    public List<BlBlockKeyEntry> Entries { get; } = new();

    public string ToMarkDown()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Entry Size: {EntrySize}");
        builder.AppendLine($"Secondary Entry Size: {SecondaryEntrySize}");
        builder.AppendLine($"Total Entry Count: {TotalEntryCount}");
        builder.AppendLine($"Used Entry Count: {UsedEntryCount}");
        builder.AppendLine();
        builder.AppendLine("| Index | Child Id | Parent Id | Tag |");
        builder.AppendLine("| --- | --- | --- | --- |");

        for (var i = 0; i < Entries.Count; i++)
        {
            var entry = Entries[i];
            builder.Append("| ")
                .Append(i)
                .Append(" | ")
                .Append(entry.ChildId)
                .Append(" | ")
                .Append(entry.ParentId)
                .Append(" | ")
                .Append(entry.Tag.Value)
                .AppendLine(" |");
        }

        return builder.ToString();
    }
}

/// <summary>
/// Describes a single relationship row captured from the <c>KEY*</c> block.
/// </summary>
internal readonly record struct BlBlockKeyEntry(int ChildId, int ParentId, BlTag Tag);

/// <summary>
/// Helper extensions that read <c>KEY*</c> tables from the movie stream.
/// </summary>
internal static class BlBlockKeyTableExtensions
{
    public static BlBlockKeyTable ReadKeyTable(this ReaderContext context, long payloadStart)
    {
        var reader = context.Reader;
        var restore = reader.Position;
        reader.Position = payloadStart;

        var entrySize = reader.ReadUInt16();
        var secondaryEntrySize = reader.ReadUInt16();
        var totalEntryCount = reader.ReadUInt32();
        var usedEntryCount = reader.ReadUInt32();

        var table = new BlBlockKeyTable
        {
            EntrySize = entrySize,
            SecondaryEntrySize = secondaryEntrySize,
            TotalEntryCount = totalEntryCount,
            UsedEntryCount = usedEntryCount
        };

        for (uint i = 0; i < table.UsedEntryCount; i++)
        {
            var entryStart = reader.Position;
            var childId = unchecked((int)reader.ReadUInt32());
            var parentId = unchecked((int)reader.ReadUInt32());
            var tag = reader.ReadTag();

            table.Entries.Add(new BlBlockKeyEntry(childId, parentId, tag));

            var consumed = reader.Position - entryStart;
            var padding = (long)table.EntrySize - consumed;
            if (padding < 0)
            {
                throw new InvalidDataException($"KEY* entry {i} consumed more bytes than the declared entry size.");
            }

            if (padding > 0)
            {
                reader.Skip(padding);
            }
        }

        reader.Position = restore;
        return table;
    }
}
