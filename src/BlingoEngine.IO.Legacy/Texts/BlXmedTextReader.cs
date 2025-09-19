using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;

namespace ProjectorRays.CastMembers;

public enum XmedAlignment
{
    Center = 0,
    Right = 1,
    Left = 2,
    Justify = 3
}

/// <summary>
/// Represents a parsed XMED styled text chunk.
/// </summary>
public sealed class XmedDocument
{
    /// <summary>Complete plain text contained in the chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Runs of text with basic style information.</summary>
    public List<XmedTextRun> Runs { get; } = new();

    /// <summary>Discovered style declarations such as fonts and colors.</summary>
    public List<XmedStyleDescriptor> Styles { get; } = new();

    /// <summary>Run map entries extracted from the directory section.</summary>
    public List<XmedRunMapEntry> RunMap { get; } = new();

    public uint Width { get; set; }
    public uint LineSpacing { get; set; }
    public uint TextLength { get; set; }
}

/// <summary>Represents a single styled run of characters.</summary>
public sealed class XmedTextRun
{
    public int Start { get; set; }
    public int Length { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty;
    public ushort FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public BlLegacyColor ForeColor { get; set; }
}

/// <summary>Simple style descriptor extracted from XMED metadata.</summary>
public sealed class XmedStyleDescriptor
{
    public ushort StyleId { get; set; }
    public string FontName { get; set; } = string.Empty;
    public byte ColorIndex { get; set; }
    public ushort FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikeout { get; set; }
    public bool Subscript { get; set; }
    public bool Superscript { get; set; }
    public bool TabbedField { get; set; }
    public bool EditableField { get; set; }
    public XmedAlignment Alignment { get; set; } = XmedAlignment.Center;
    public XmedAlignment AlignmentFromFlags { get; set; } = XmedAlignment.Center;
    public bool WrapOff { get; set; }
    public bool HasTabs { get; set; }
    public byte AlignmentRaw { get; set; }
    public byte StyleFlags { get; set; }
}

/// <summary>Represents a run map entry containing indices and descriptor links.</summary>
public sealed class XmedRunMapEntry
{
    public XmedRunMapEntry(ushort start, ushort f2, ushort length, ushort f4, ushort styleId, long position)
    {
        Start = start;
        F2 = f2;
        Length = length;
        F4 = f4;
        StyleId = styleId;
        Position = position;
    }

    public ushort Start { get; }
    public ushort F2 { get; }
    public ushort Length { get; }
    public ushort F4 { get; }
    public ushort StyleId { get; }
    public long Position { get; }
}

/// <summary>Directory containing the ASCII header entries found in an XMED chunk.</summary>
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

    IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();

    public XmedDirEntry? FindEntry(string type)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (string.Equals(entry.Type, type, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }
}

/// <summary>Single directory token describing offsets and counts within the XMED header.</summary>
public sealed class XmedDirEntry
{
    public XmedDirEntry(string type, long offset, int count, long position, long? dataOffset, byte terminator)
    {
        Type = type;
        Offset = offset;
        Count = count;
        Position = position;
        DataOffset = dataOffset;
        Terminator = terminator;
    }

    public string Type { get; }
    public long Offset { get; }
    public int Count { get; }
    public long Position { get; }
    public long? DataOffset { get; }
    public byte Terminator { get; }
}

/// <summary>
/// Reader for Director XMED chunks. The implementation is based on reverse-engineered
/// offsets gathered from the accompanying sample set. It extracts plain text, style runs,
/// directory information, and style descriptor blocks so that downstream tooling can
/// experiment with the data without fully understanding every historical variant yet.
/// </summary>
public sealed class BlXmedTextReader
{
    private static readonly byte[] FontMarker = { (byte)'4', (byte)'0', (byte)',' };

    /// <summary>Reads a complete XMED document from a byte buffer.</summary>
    public XmedDocument Read(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        using var stream = new MemoryStream(buffer, writable: false);
        return Read(stream);
    }

    /// <summary>Reads a complete XMED document from a stream.</summary>
    public XmedDocument Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = ReadAllBytes(stream);
        var directory = ReadHeaderDirectory(buffer);
        var (_, _, textData) = ReadTextBlock(directory);

        var header = directory.Buffer;
        var width = header.ReadUInt32LittleEndian(0x0018);
        var styleFlags = header.ReadByteOrDefault(0x001C);
        var alignFlags = header.ReadByteOrDefault(0x001D);
        var lineSpacing = header.ReadUInt32LittleEndian(0x003C);
        var fontSize = (ushort)Math.Clamp(header.ReadUInt32LittleEndian(0x0040), 0, 0xFFFF);
        var headerTextLen = header.ReadUInt32LittleEndian(0x004C);

        var document = new XmedDocument
        {
            Width = width,
            LineSpacing = lineSpacing,
            TextLength = headerTextLen
        };

        var baseStyle = new XmedStyleDescriptor
        {
            FontSize = fontSize,
            AlignmentRaw = alignFlags,
            StyleFlags = styleFlags
        };
        ApplyStyleFlags(styleFlags, baseStyle);
        ApplyAlignmentFlags(alignFlags, baseStyle);
        document.Styles.Add(baseStyle);

        var text = Encoding.Latin1.GetString(textData.Span);
        document.Text = text;

        var runMap = ReadRunMap(directory);
        foreach (var entry in runMap)
        {
            document.RunMap.Add(entry);
        }

        var descriptors = ReadStyleDescriptors(directory);
        foreach (var descriptor in descriptors)
        {
            document.Styles.Add(descriptor);
        }

        foreach (var mapEntry in runMap.OrderBy(r => r.Start))
        {
            var start = mapEntry.Start;
            var length = mapEntry.Length;

            if (start < 0 || length <= 0 || start + length > text.Length)
            {
                continue;
            }

            var run = new XmedTextRun
            {
                Start = start,
                Length = length,
                Text = text.Substring(start, length),
                FontSize = baseStyle.FontSize,
                Bold = baseStyle.Bold,
                Italic = baseStyle.Italic,
                Underline = baseStyle.Underline,
                ForeColor = new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex)
            };

            if (TryGetDescriptor(descriptors, mapEntry.StyleId, out var descriptor))
            {
                if (!string.IsNullOrEmpty(descriptor.FontName))
                {
                    run.FontName = descriptor.FontName;
                }

                if (descriptor.ColorIndex != 0)
                {
                    run.ForeColor = new BlLegacyColor(descriptor.ColorIndex, descriptor.ColorIndex, descriptor.ColorIndex);
                }

                if (descriptor.FontSize != 0)
                {
                    run.FontSize = descriptor.FontSize;
                }

                run.Bold = descriptor.Bold;
                run.Italic = descriptor.Italic;
                run.Underline = descriptor.Underline;
            }
            else
            {
                run.FontName = baseStyle.FontName;
            }

            document.Runs.Add(run);
        }

        if (document.Runs.Count == 0 && text.Length > 0)
        {
            document.Runs.Add(new XmedTextRun
            {
                Start = 0,
                Length = text.Length,
                Text = text,
                FontName = baseStyle.FontName,
                FontSize = baseStyle.FontSize,
                Bold = baseStyle.Bold,
                Italic = baseStyle.Italic,
                Underline = baseStyle.Underline,
                ForeColor = new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex)
            });
        }

        return document;
    }

    /// <summary>Scans the ASCII directory tokens (20 chars) in the header.</summary>
    public XmedDir ReadHeaderDirectory(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        var entries = new List<XmedDirEntry>();

        int headerLength = Array.IndexOf(buffer, (byte)0x00);
        if (headerLength < 0)
        {
            headerLength = buffer.Length;
        }

        for (int i = 0; i + 20 <= buffer.Length; i++)
        {
            ReadOnlySpan<byte> span = buffer.AsSpan(i, 20);
            if (!span.IsAsciiHexOrDigits())
            {
                continue;
            }

            var token = Encoding.ASCII.GetString(span);
            var type = token[..4];
            if (!type.IsAsciiHexOrDigitString())
            {
                continue;
            }

            long offset = token.AsSpan(4, 8).ParseHexInt64();
            int count = (int)token.AsSpan(12, 8).ParseHexInt64();
            byte terminator = i + 20 < buffer.Length ? buffer[i + 20] : (byte)0;
            long? dataOffset = terminator == 0x00 ? i + 21 : null;

            entries.Add(new XmedDirEntry(type, offset, count, i, dataOffset, terminator));
        }

        return new XmedDir(buffer, entries, headerLength);
    }

    /// <summary>Reads the text block pointed by directory entry 0002.</summary>
    public (long offset, int length, ReadOnlyMemory<byte> data) ReadTextBlock(XmedDir directory)
    {
        var textEntry = directory.FindEntry("0002") ?? throw new InvalidOperationException("XMED text entry (0002) not found");
        if (textEntry.DataOffset is null)
        {
            throw new InvalidOperationException("Text entry missing data offset");
        }

        var buffer = directory.Buffer;
        int cursor = (int)textEntry.DataOffset.Value;
        int end = buffer.Length;

        int lenStart = cursor;
        while (cursor < end && buffer[cursor] != (byte)',')
        {
            if (!buffer[cursor].IsDigitOrHex())
            {
                throw new InvalidDataException("Unexpected char in text length");
            }

            cursor++;
        }

        if (cursor >= end || buffer[cursor] != (byte)',')
        {
            throw new InvalidDataException("Missing comma after text length");
        }

        ReadOnlySpan<byte> lenSpan = buffer.AsSpan(lenStart, cursor - lenStart);
        cursor++;

        int length = lenSpan.TryParseUInt32Decimal(out var decLen)
            ? (int)decLen
            : lenSpan.TryParseUInt32Hex(out var hexLen)
                ? (int)hexLen
                : throw new InvalidDataException("Cannot parse text length");

        if (cursor + length > end)
        {
            throw new InvalidDataException("Text block exceeds buffer");
        }

        return (cursor, length, buffer.AsMemory(cursor, length));
    }

    /// <summary>Reads run map entries (0004–0007) and returns a normalized list.</summary>
    public IReadOnlyList<XmedRunMapEntry> ReadRunMap(XmedDir directory)
    {
        var list = new List<XmedRunMapEntry>();
        foreach (var entry in directory)
        {
            if (!IsRunType(entry.Type))
            {
                continue;
            }

            if (entry.Position < 0 || entry.Position + 20 > directory.Buffer.Length)
            {
                continue;
            }

            ReadOnlySpan<byte> span = directory.Buffer.AsSpan((int)entry.Position, 20);
            if (!span.IsDigits())
            {
                continue;
            }

            int f1 = span.Slice(0, 4).ParseInt32();
            int f2 = span.Slice(4, 4).ParseInt32();
            int len = span.Slice(8, 4).ParseInt32();
            int f4 = span.Slice(12, 4).ParseInt32();
            int sid = span.Slice(16, 4).ParseInt32();

            list.Add(new XmedRunMapEntry((ushort)f1, (ushort)f2, (ushort)len, (ushort)f4, (ushort)sid, entry.Position));
        }

        list.Sort((a, b) => a.Start.CompareTo(b.Start));
        return list;
    }

    /// <summary>Parses style descriptors from clustered blocks after the header.</summary>
    public IReadOnlyList<XmedStyleDescriptor> ReadStyleDescriptors(XmedDir directory)
    {
        var buffer = directory.Buffer;
        var styles = new List<XmedStyleDescriptor>();

        for (int i = directory.HeaderLength; i + 8 < buffer.Length; i++)
        {
            byte styleByte = buffer[i + 0];
            byte flagsByte = buffer[i + 1];

            ReadOnlySpan<byte> idSpan = buffer.AsSpan(i + 2, 4);
            if (!idSpan.IsDigits())
            {
                continue;
            }

            int styleId = idSpan.ParseInt32();

            int markerIndex = buffer.IndexOfSequence(i + 6, FontMarker);
            if (markerIndex < 0)
            {
                continue;
            }

            int nameStart = markerIndex + FontMarker.Length;
            int colorIndex = 0;
            if (nameStart < buffer.Length)
            {
                colorIndex = buffer[nameStart];
                nameStart++;
            }

            int nameEnd = nameStart;
            while (nameEnd < buffer.Length && buffer[nameEnd] != 0x00 && buffer[nameEnd].IsPrintable())
            {
                nameEnd++;
            }

            if (nameEnd <= nameStart || nameEnd >= buffer.Length)
            {
                continue;
            }

            var descriptor = new XmedStyleDescriptor
            {
                StyleId = (ushort)styleId,
                AlignmentRaw = flagsByte,
                StyleFlags = styleByte,
                FontName = Encoding.Latin1.GetString(buffer, nameStart, nameEnd - nameStart),
                ColorIndex = (byte)colorIndex,
                FontSize = 0
            };

            ApplyStyleFlags(styleByte, descriptor);
            ApplyAlignmentFlags(flagsByte, descriptor);

            if (!styles.Any(s => s.StyleId == descriptor.StyleId))
            {
                styles.Add(descriptor);
            }

            i = nameEnd;
        }

        return styles;
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek)
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            memory.Position = 0;

            var memoryReader = new BlStreamReader(memory);
            if (memoryReader.Length > int.MaxValue)
            {
                throw new InvalidDataException("XMED streams larger than 2 GB are not supported.");
            }

            var length = (int)memoryReader.Length;
            var buffer = new byte[length];
            memoryReader.ReadExactly(buffer);
            return buffer;
        }

        var reader = new BlStreamReader(stream);
        long originalPosition = reader.Position;
        reader.Position = 0;

        if (reader.Length > int.MaxValue)
        {
            throw new InvalidDataException("XMED streams larger than 2 GB are not supported.");
        }

        var size = (int)reader.Length;
        var result = new byte[size];
        reader.ReadExactly(result);
        reader.Position = originalPosition;
        return result;
    }

    private static void ApplyStyleFlags(byte flags, XmedStyleDescriptor style)
    {
        style.Bold = (flags & 0x01) != 0;
        style.Italic = (flags & 0x02) != 0;
        style.Underline = (flags & 0x04) != 0;
        style.Strikeout = (flags & 0x08) != 0;
        style.Subscript = (flags & 0x10) != 0;
        style.Superscript = (flags & 0x20) != 0;
        style.TabbedField = (flags & 0x40) != 0;
        style.EditableField = (flags & 0x80) != 0;
    }

    private static void ApplyAlignmentFlags(byte flags, XmedStyleDescriptor style)
    {
        style.WrapOff = (flags & 0x08) != 0;
        style.HasTabs = (flags & 0x10) != 0;
        style.AlignmentFromFlags = (XmedAlignment)(flags & 0x03);

        style.Alignment = (flags & 0x03) switch
        {
            0x01 => XmedAlignment.Right,
            0x02 => XmedAlignment.Left,
            0x03 => XmedAlignment.Justify,
            _ => XmedAlignment.Center
        };
    }

    private static bool TryGetDescriptor(IReadOnlyList<XmedStyleDescriptor> descriptors, int id, out XmedStyleDescriptor descriptor)
    {
        for (int i = 0; i < descriptors.Count; i++)
        {
            if (descriptors[i].StyleId == (ushort)id)
            {
                descriptor = descriptors[i];
                return true;
            }
        }

        descriptor = default!;
        return false;
    }

    private static bool IsRunType(string type) => type is "0004" or "0005" or "0006" or "0007";
}
