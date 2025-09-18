using System.Collections.Generic;
using System.Text;

using BlingoEngine.IO.Legacy;

namespace BlingoEngine.IO.Legacy.Classic.Blocks;

/// <summary>
/// Represents the rows stored inside a <c>KEY*</c> block.
/// </summary>
internal sealed class BlBlockKeyTable
{
    public ushort EntrySize { get; init; }
    public uint EntryCount { get; init; }
    public List<BlBlockKeyEntry> Entries { get; } = new();

    public string ToMarkDown()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Entry Size: {EntrySize}");
        builder.AppendLine($"Entry Count: {EntryCount}");
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

        var table = new BlBlockKeyTable
        {
            EntrySize = reader.ReadUInt16(),
            EntryCount = reader.ReadUInt32()
        };

        for (uint i = 0; i < table.EntryCount; i++)
        {
            var entryStart = reader.Position;
            var childId = unchecked((int)reader.ReadUInt32());
            var parentId = unchecked((int)reader.ReadUInt32());
            var tag = reader.ReadTag();

            table.Entries.Add(new BlBlockKeyEntry(childId, parentId, tag));

            var consumed = reader.Position - entryStart;
            var padding = (long)table.EntrySize - consumed;
            if (padding > 0)
            {
                reader.Skip(padding);
            }
        }

        reader.Position = restore;
        return table;
    }
}
