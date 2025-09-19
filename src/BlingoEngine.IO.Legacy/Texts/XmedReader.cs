using System.Buffers;
using System.Collections;
using System.Globalization;
using System.Text;

namespace ProjectorRays.CastMembers;

public sealed record XmedDirEntry(string Type, long Offset, int Count, long Position, long? DataOffset, byte Terminator);

public sealed class XmedDir : IReadOnlyList<XmedDirEntry>
{
    private readonly List<XmedDirEntry> _entries;

    public XmedDir(byte[] buffer, List<XmedDirEntry> entries, int headerLength)
    {
        Buffer = buffer;
        _entries = entries;
        HeaderLength = headerLength;
    }

    public byte[] Buffer { get; }

    public int HeaderLength { get; }

    public int Count => _entries.Count;

    public XmedDirEntry this[int index] => _entries[index];

    public IEnumerator<XmedDirEntry> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public XmedDirEntry? FindEntry(string type) => _entries.FirstOrDefault(e => string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase));
}

public sealed record XmedFontRecord(byte Length, string Name, long Offset);

public sealed record XmedRunMapEntry(ushort F1, ushort F2, ushort Length, ushort F4, ushort StyleId, long Offset);

public sealed record XmedStyleDescriptor(
    ushort StyleId,
    string FontName,
    int FontPx,
    byte Flags,
    byte Align,
    int? LetterSpacing,
    int? LineSpacing,
    int? ColorIndex,
    long Offset);

public sealed class XmedReader
{
    public XmedDir ReadHeaderDirectory(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanSeek)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            stream = new MemoryStream(ms.ToArray());
        }

        stream.Position = 0;
        var buffer = new byte[stream.Length];
        var read = stream.Read(buffer, 0, buffer.Length);
        if (read != buffer.Length)
            throw new IOException("Unable to read XMED buffer");

        stream.Position = 0;

        var entries = new List<XmedDirEntry>();

        int headerLength = Array.IndexOf(buffer, (byte)0x00);
        if (headerLength < 0)
            headerLength = buffer.Length;

        for (int i = 0; i <= buffer.Length - 20; i++)
        {
            if (!IsHexBlock(buffer.AsSpan(i, 20)))
                continue;

            string token = Encoding.ASCII.GetString(buffer, i, 20);
            string type = token[..4];
            long offset = ParseHex64(token.AsSpan(4, 8));
            int count = (int)ParseHex64(token.AsSpan(12, 8));
            byte terminator = i + 20 < buffer.Length ? buffer[i + 20] : (byte)0;
            long? dataOffset = terminator switch
            {
                0x00 => i + 21,
                _ => null
            };

            entries.Add(new XmedDirEntry(type, offset, count, i, dataOffset, terminator));
        }

        return new XmedDir(buffer, entries, headerLength);
    }

    public (long offset, int length, ReadOnlyMemory<byte> data) ReadTextBlock(Stream stream, XmedDir directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        var textEntry = directory.FindEntry("0002") ?? throw new InvalidOperationException("Text entry not found in XMED header");
        if (textEntry.DataOffset is null)
            throw new InvalidOperationException("Text entry is missing data offset");

        var buffer = directory.Buffer;
        int cursor = (int)textEntry.DataOffset.Value;
        int end = buffer.Length;
        var digits = new ArrayBufferWriter<byte>();

        while (cursor < end)
        {
            byte b = buffer[cursor];
            if (b == (byte)',')
            {
                cursor++;
                break;
            }

            if (IsAsciiHex(b))
            {
                digits.Write(new[] { b });
                cursor++;
                continue;
            }

            throw new InvalidDataException("Unexpected character while reading text length");
        }

        if (digits.WrittenCount == 0)
            throw new InvalidDataException("Missing text length token");

        int length = (int)ParseHex64(digits.WrittenSpan);
        if (cursor + length > end)
            throw new InvalidDataException("Text block extends beyond the buffer");

        return (cursor, length, directory.Buffer.AsMemory(cursor, length));
    }

    public IReadOnlyList<XmedFontRecord> ReadFontTable(Stream stream, XmedDirEntry fontEntry)
    {
        if (fontEntry is null)
            throw new ArgumentNullException(nameof(fontEntry));

        if (fontEntry.DataOffset is null)
            throw new InvalidOperationException("Font table entry has no data offset");

        var buffer = ReadBuffer(stream);
        return ReadFontRecords(buffer, fontEntry);
    }

    public IReadOnlyList<XmedRunMapEntry> ReadRunMap(Stream stream, XmedDir directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        var entries = new List<XmedRunMapEntry>();

        foreach (var entry in directory)
        {
            if (!IsRunEntry(entry.Type))
                continue;

            var token = directory.Buffer.AsSpan((int)entry.Position, 20);
            ushort f1 = (ushort)ParseHex64(token.Slice(0, 4));
            ushort f2 = (ushort)ParseHex64(token.Slice(4, 4));
            ushort len = (ushort)ParseHex64(token.Slice(8, 4));
            ushort f4 = (ushort)ParseHex64(token.Slice(12, 4));
            ushort style = (ushort)ParseHex64(token.Slice(16, 4));
            entries.Add(new XmedRunMapEntry(f1, f2, len, f4, style, entry.Position));
        }

        entries.Sort((a, b) => a.F1.CompareTo(b.F1));
        return entries;
    }

    public IReadOnlyList<XmedStyleDescriptor> ReadStyleDescriptors(Stream stream, XmedDir directory)
    {
        // Placeholder implementation: descriptor decoding remains an open task.
        return Array.Empty<XmedStyleDescriptor>();
    }

    private static bool IsHexBlock(ReadOnlySpan<byte> span)
    {
        if (span.Length != 20)
            return false;

        foreach (var b in span)
        {
            if (!IsAsciiHex(b))
                return false;
        }

        return true;
    }

    private static bool IsAsciiHex(byte b) =>
        (b >= (byte)'0' && b <= (byte)'9') ||
        (b >= (byte)'A' && b <= (byte)'F') ||
        (b >= (byte)'a' && b <= (byte)'f');

    private static long ParseHex64(ReadOnlySpan<char> span) => long.Parse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private static long ParseHex64(ReadOnlySpan<byte> span)
    {
        Span<char> chars = stackalloc char[span.Length];
        for (int i = 0; i < span.Length; i++)
            chars[i] = (char)span[i];
        return ParseHex64(chars);
    }

    private static IReadOnlyList<XmedFontRecord> ReadFontRecords(byte[] buffer, XmedDirEntry fontEntry)
    {
        int offset = (int)fontEntry.Position;
        var records = new List<XmedFontRecord>();
        ReadOnlySpan<byte> marker = stackalloc byte[] { (byte)'4', (byte)'0', (byte)',' };

        while (records.Count < fontEntry.Count && offset < buffer.Length)
        {
            var search = buffer.AsSpan(offset);
            int markerIndex = search.IndexOf(marker);
            if (markerIndex < 0)
                break;

            offset += markerIndex;
            if (offset + 3 >= buffer.Length)
                break;

            byte length = buffer[offset + 3];
            int nameStart = offset + 4;
            int nameEnd = nameStart + length;
            if (nameEnd > buffer.Length)
                throw new InvalidDataException("Font record extends beyond buffer bounds");

            if (length > 0)
            {
                string name = Encoding.Latin1.GetString(buffer, nameStart, length);
                records.Add(new XmedFontRecord(length, name, offset));
            }

            offset = nameEnd;
        }

        return records;
    }

    private static byte[] ReadBuffer(Stream stream)
    {
        if (stream is MemoryStream ms)
        {
            var result = ms.ToArray();
            ms.Position = 0;
            return result;
        }

        long origin = stream.CanSeek ? stream.Position : 0;
        if (stream.CanSeek)
            stream.Position = 0;

        using var copy = new MemoryStream();
        stream.CopyTo(copy);

        if (stream.CanSeek)
            stream.Position = origin;

        return copy.ToArray();
    }

    private static bool IsRunEntry(string type)
    {
        if (type.Length != 4)
            return false;

        return type switch
        {
            "0004" => true,
            "0005" => true,
            "0006" => true,
            "0007" => true,
            _ => false
        };
    }
}
