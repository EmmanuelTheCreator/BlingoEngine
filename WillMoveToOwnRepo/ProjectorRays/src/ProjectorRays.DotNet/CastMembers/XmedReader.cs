using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using ProjectorRays.Common;
using static ProjectorRays.CastMembers.XmedChunkParser;

namespace ProjectorRays.CastMembers;

public enum XmedAlignment
{
    Center,
    Left,
    Right
}

/// <summary>
/// Represents a parsed XMED styled text chunk.
/// </summary>
public sealed class XmedDocument
{
    /// <summary>Complete plain text contained in the chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Runs of text with basic style information.</summary>
    public List<TextStyleRun> Runs { get; } = new();

    /// <summary>Discovered style declarations such as fonts and colors.</summary>
    public List<XmedStyleDeclaration> Styles { get; } = new();

    /// <summary>Style map entries describing line ranges.</summary>
    public List<XmedStyleMapEntry> MapEntries { get; } = new();

    public uint Width { get; set; }
    public uint LineSpacing { get; set; }
    public uint TextLength { get; set; }
}

/// <summary>Simple style declaration extracted from XMED.</summary>
public sealed class XmedStyleDeclaration
{
    public ushort StyleId { get; set; }
    public ushort BaseStyleId { get; set; }
    public ushort F2 { get; set; }
    public ushort F4 { get; set; }
    public ushort TextLength { get; set; }
    public string FontName { get; set; } = string.Empty;
    public byte ColorIndex { get; set; } = 0;
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
    public bool WrapOff { get; set; }
    public bool HasTabs { get; set; }
    public byte AlignmentRaw { get; set; }
    public byte[] UnknownHeader { get; set; } = Array.Empty<byte>();
}

public sealed class XmedStyleMapEntry
{
    public ushort StyleId { get; set; }
    public ushort F2 { get; set; }
    public ushort TextLength { get; set; }
    public ushort F4 { get; set; }
    public ushort BaseStyleId { get; set; }
}

/// <summary>
/// Basic reader for Director XMED chunks.  This is a best-effort
/// implementation based on observed sample files.  The format is not fully
/// documented, but this reader extracts plain text and the most obvious style
/// information so that callers can experiment with the data.
/// </summary>
public class XmedReader : IXmedReader
{
    public XmedDocument Read(BufferView view)
    {
        var data = view.Data;
        int scanStart = view.Offset;
        int end = scanStart + view.Size;

        int start = -1;
        for (int scan = scanStart; scan <= end - 4; scan++)
        {
            if (data[scan] == (byte)'D' && data[scan + 1] == (byte)'E' && data[scan + 2] == (byte)'M' && data[scan + 3] == (byte)'X')
            {
                start = scan;
                break;
            }
        }

        if (start < 0)
            throw new InvalidDataException("Invalid XMED chunk header");

        ushort fontSize = 0;
        if (start - 0x14 >= scanStart)
            fontSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(start - 0x14));

        var doc = new XmedDocument();
        var textBuilder = new StringBuilder();

        // Basic style information stored near the start of the chunk.  Offsets
        // are derived from XMED_Offsets.md.
        doc.Width = BitConverter.ToUInt32(data, start + 0x18);
        byte styleFlags = data[start + 0x1C];
        byte alignByte = data[start + 0x1D];
        doc.LineSpacing = BitConverter.ToUInt32(data, start + 0x3C);
        if (fontSize == 0)
            fontSize = BitConverter.ToUInt16(data, start + 0x40);
        doc.TextLength = BitConverter.ToUInt32(data, start + 0x4C);

        var baseStyle = new XmedStyleDeclaration
        {
            FontSize = fontSize,
            AlignmentRaw = alignByte
        };
        ApplyStyleFlags(styleFlags, baseStyle);
        ApplyAlignmentFlags(alignByte, baseStyle);
        doc.Styles.Add(baseStyle);

        // Parse sequentially after the header.  The first 4 bytes are "DEMX" so
        // begin scanning at offset+4.
        int i = start + 4;
        XmedStyleDeclaration? currentStyle = null;

        while (i < end)
        {
            byte b = data[i];

            // Pattern: "40," + color + font name + NUL" (font table entries)
            if (b == (byte)'4' && i + 3 < end && data[i + 1] == (byte)'0' && data[i + 2] == (byte)',')
            {
                byte color = data[i + 3];
                int j = i + 4;
                while (j < end && IsPrintable(data[j])) j++;
                string font = Encoding.Latin1.GetString(data, i + 4, j - (i + 4));

                if (font.Length == 0)
                {
                    currentStyle = null;
                }
                else
                {
                    currentStyle = new XmedStyleDeclaration
                    {
                        FontName = font,
                        ColorIndex = color,
                        FontSize = fontSize
                    };
                    doc.Styles.Add(currentStyle);
                }

                i = j;
                if (i < end && data[i] == 0) i++;
                continue;
            }

            if (IsDigit(b))
            {
                int j = i;
                while (j < end && IsDigit(data[j])) j++;
                int digitLen = j - i;
                if (digitLen == 20 && j < end && data[j] == 0x00)
                {
                    if (j + 3 < end && data[j + 1] == (byte)'4' && data[j + 2] == (byte)'0' && data[j + 3] == (byte)',')
                    {
                        if (i >= start + 7)
                        {
                            byte sFlags = data[i - 7];
                            byte aByte = data[i - 6];
                            var header = new byte[5];
                            Array.Copy(data, i - 5, header, 0, 5);
                            string digits = Encoding.ASCII.GetString(data, i, 20);
                            var styleDecl = new XmedStyleDeclaration
                            {
                                StyleId = ushort.Parse(digits.Substring(0, 4)),
                                F2 = ushort.Parse(digits.Substring(4, 4)),
                                TextLength = ushort.Parse(digits.Substring(8, 4)),
                                F4 = ushort.Parse(digits.Substring(12, 4)),
                                BaseStyleId = ushort.Parse(digits.Substring(16, 4)),
                                ColorIndex = data[j + 3],
                                FontSize = fontSize,
                                AlignmentRaw = aByte,
                                UnknownHeader = header
                            };
                            int fontStart = j + 4;
                            int fontEnd = fontStart;
                            while (fontEnd < end && IsPrintable(data[fontEnd])) fontEnd++;
                            styleDecl.FontName = Encoding.Latin1.GetString(data, fontStart, fontEnd - fontStart);

                            ApplyStyleFlags(sFlags, styleDecl);
                            ApplyAlignmentFlags(aByte, styleDecl);
                            doc.Styles.Add(styleDecl);
                            currentStyle = styleDecl;

                            i = fontEnd;
                            if (i < end && data[i] == 0) i++;
                            continue;
                        }
                    }
                    else
                    {
                        string digits = Encoding.ASCII.GetString(data, i, 20);
                        var entry = new XmedStyleMapEntry
                        {
                            StyleId = ushort.Parse(digits.Substring(0, 4)),
                            F2 = ushort.Parse(digits.Substring(4, 4)),
                            TextLength = ushort.Parse(digits.Substring(8, 4)),
                            F4 = ushort.Parse(digits.Substring(12, 4)),
                            BaseStyleId = ushort.Parse(digits.Substring(16, 4))
                        };
                        doc.MapEntries.Add(entry);
                        i = j;
                        if (i < end && data[i] == 0) i++;
                        continue;
                    }
                }

                if (j < end && data[j] == 0x2C)
                {
                    string num = Encoding.ASCII.GetString(data, i, j - i);
                    if (int.TryParse(num, out int len))
                    {
                        int textStart = j + 1;
                        if (textStart + len <= end)
                        {
                            bool printable = true;
                            for (int k = 0; k < len; k++)
                            {
                                if (!IsPrintable(data[textStart + k]))
                                {
                                    printable = false;
                                    break;
                                }
                            }

                            var run = new TextStyleRun
                            {
                                Length = len,
                                Start = textBuilder.Length,
                                FontSize = fontSize,
                                Bold = baseStyle.Bold,
                                Italic = baseStyle.Italic,
                                Underline = baseStyle.Underline
                            };

                            if (currentStyle != null)
                            {
                                run.FontName = currentStyle.FontName;
                                run.ForeColor = new RayColor(currentStyle.ColorIndex, currentStyle.ColorIndex, currentStyle.ColorIndex);
                            }

                            if (printable)
                            {
                                string text = Encoding.Latin1.GetString(data, textStart, len);
                                run.Text = text;
                                textBuilder.Append(text);
                            }
                            else
                            {
                                var span = data.AsSpan(textStart, len);
                                if (len >= 2)
                                    run.Unknown1 = BinaryPrimitives.ReadUInt16BigEndian(span);
                                if (len >= 6)
                                    run.Unknown2 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(2));
                                if (len >= 10)
                                    run.Unknown3 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(6));
                                if (len >= 14)
                                    run.Unknown4 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(10));
                                if (len >= 18)
                                    run.Unknown5 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(14));
                                if (len >= 22)
                                    run.Unknown6 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(18));
                                if (len >= 26)
                                    run.Unknown7 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(22));
                                if (len >= 30)
                                    run.Unknown8 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(26));
                                if (len >= 34)
                                    run.Unknown9 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(30));
                                if (len >= 38)
                                    run.Unknown10 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(34));
                                if (len >= 42)
                                    run.Unknown11 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(38));
                            }

                            doc.Runs.Add(run);

                            i = textStart + len;
                            if (i < end && (data[i] == 0x00 || data[i] == 0x03))
                                i++;
                            continue;
                        }
                    }
                }
            }

            i++;
        }

        if (doc.Runs.Count > 0 && string.IsNullOrEmpty(doc.Runs[0].FontName) && doc.Styles.Count > 1)
        {
            var style = doc.Styles[^1];
            doc.Runs[0].FontName = style.FontName;
            doc.Runs[0].ForeColor = new RayColor(style.ColorIndex, style.ColorIndex, style.ColorIndex);
        }

        doc.Text = textBuilder.ToString();
        return doc;
    }

    private static void ApplyStyleFlags(byte flags, XmedStyleDeclaration style)
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

    private static void ApplyAlignmentFlags(byte b, XmedStyleDeclaration style)
    {
        style.WrapOff = b == 0x19;
        style.HasTabs = (b & 0x10) != 0;
        style.Alignment = b switch
        {
            0x1A => XmedAlignment.Left,
            0x15 => XmedAlignment.Right,
            _ => XmedAlignment.Center
        };
    }

    private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

    private static bool IsPrintable(byte b) => b >= 32 && b <= 126;
}

