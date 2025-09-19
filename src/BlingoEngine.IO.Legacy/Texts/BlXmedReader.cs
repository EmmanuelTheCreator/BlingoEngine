using BlingoEngine.IO.Legacy.Core;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace ProjectorRays.CastMembers;

public enum BlXmedAlignment
{
    Center,
    Left,
    Right,
    Justify
}
public class BlTextStyleRun
{
    public int Start { get; set; }
    public int Length { get; set; }
    public string FontName { get; set; } = "";
    public ushort FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string Text { get; set; } = "";
    public ushort FontId { get; set; }
    public BlLegacyColor ForeColor { get; set; }
    public BlLegacyColor BackColor { get; set; }
    public ushort Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
    public uint Unknown3 { get; set; }
    public uint Unknown4 { get; set; }
    public uint Unknown5 { get; set; }
    public uint Unknown6 { get; set; }
    public uint Unknown7 { get; set; }
    public uint Unknown8 { get; set; }
    public uint Unknown9 { get; set; }
    public uint Unknown10 { get; set; }
    public uint Unknown11 { get; set; }
}

/// <summary>
/// Represents a parsed XMED styled text chunk.
/// </summary>
public sealed class XmedDocument
{
    /// <summary>Complete plain text contained in the chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Runs of text with basic style information.</summary>
    public List<BlTextStyleRun> Runs { get; } = new();

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
    public bool Locked { get; set; }
    public BlXmedAlignment Alignment { get; set; } = BlXmedAlignment.Center;
    public BlXmedAlignment AlignmentFromFlags { get; set; } = BlXmedAlignment.Center;
    public bool WrapOff { get; set; }
    public bool HasTabs { get; set; }
    public byte AlignmentRaw { get; set; }
    public byte? AlignmentMarker { get; set; }
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
public class BlXmedReader
{
    public XmedDocument Read(byte[] view)
    {
        var data = view ?? Array.Empty<byte>();
        var start = 0;
        var end = data.Length;

        const int OFF_WIDTH = 0x0018;
        const int OFF_STYLE = 0x001C;
        const int OFF_ALIGN = 0x001D;
        const int OFF_LINESP = 0x003C;
        const int OFF_FONTSIZE = 0x0040; // int32 LE (validated)
        const int OFF_TEXTLEN = 0x004C; // int32 LE (validated)

        static uint ReadU32LE(byte[] s, int o) =>
            (o >= 0 && o + 4 <= s.Length) ? BinaryPrimitives.ReadUInt32LittleEndian(s.AsSpan(o, 4)) : 0u;

        static byte ReadU8(byte[] s, int o) =>
            (o >= 0 && o < s.Length) ? s[o] : (byte)0;

        var doc = new XmedDocument();
        var textBuilder = new StringBuilder();

        // --- header / fixed offsets (validated) ---
        doc.Width = ReadU32LE(data, start + OFF_WIDTH);
        byte styleFlags = ReadU8(data, start + OFF_STYLE);
        byte alignByte = ReadU8(data, start + OFF_ALIGN);
        doc.LineSpacing = ReadU32LE(data, start + OFF_LINESP);
        // font size stored as int32, we clamp to ushort field
        ushort fontSize;
        if (!TryReadAsciiHexUInt16(data, start + OFF_FONTSIZE, out fontSize))
        {
            fontSize = (ushort)ReadU32LE(data, start + OFF_FONTSIZE);
        }
        doc.TextLength = ReadU32LE(data, start + OFF_TEXTLEN);

        var baseStyle = new XmedStyleDeclaration
        {
            FontSize = fontSize,
            AlignmentRaw = alignByte
        };
        ApplyStyleFlags(styleFlags, baseStyle);
        ApplyAlignmentFlags(alignByte, baseStyle);

        byte detectedAlignmentCode = DetectAlignmentCode(data, start, Math.Min(end, start + 0x200));
        if (detectedAlignmentCode != 0 && TryDecodeAlignment(detectedAlignmentCode, out var detectedAlignment))
        {
            baseStyle.AlignmentMarker = detectedAlignmentCode;
            baseStyle.Alignment = detectedAlignment;
        }

        doc.Styles.Add(baseStyle);

        // --- sequential scan ---
        int i = start;
        XmedStyleDeclaration? currentStyle = null;

        while (i < end)
        {
            byte b = data[i];

            // "40," font table entry: [ '4','0',',' , colorByte , asciiFontName , 0x00 ]
            if (b == (byte)'4' && i + 3 < end && data[i + 1] == (byte)'0' && data[i + 2] == (byte)',')
            {
                byte color = data[i + 3];
                int j = i + 4;
                while (j < end && IsPrintable(data[j])) j++;
                string font = Encoding.Latin1.GetString(data, i + 4, Math.Max(0, j - (i + 4)));

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

            // 20 ASCII digits → either style map or descriptor header
            if (IsHexDigit(b))
            {
                int j = i;
                while (j < end && IsHexDigit(data[j])) j++;
                int digitLen = j - i;

                if (digitLen == 20 && IsAllDecimalDigits(data, i, digitLen))
                {
                    // If followed by NUL and immediately by "40,", treat as descriptor header
                    bool hasNull = (j < end && data[j] == 0x00);
                    bool fontAfter = (j + 3 < end && data[j + 1] == (byte)'4' && data[j + 2] == (byte)'0' && data[j + 3] == (byte)',');

                    if (hasNull && fontAfter && i >= start + 7)
                    {
                        byte sFlags = data[i - 7];
                        byte aByte = data[i - 6];
                        var header = new byte[Math.Min(5, end - (i - 5))];
                        if (header.Length == 5) Array.Copy(data, i - 5, header, 0, 5);

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
                        styleDecl.FontName = Encoding.Latin1.GetString(data, fontStart, Math.Max(0, fontEnd - fontStart));

                        ApplyStyleFlags(sFlags, styleDecl);
                        ApplyAlignmentFlags(aByte, styleDecl);
                        doc.Styles.Add(styleDecl);
                        currentStyle = styleDecl;

                        i = fontEnd;
                        if (i < end && data[i] == 0) i++;
                        continue;
                    }
                    else
                    {
                        // Style map entry "00040000002900000008" etc.
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

                // "<len>,<ascii...>"
                if (j < end && data[j] == 0x2C)
                {
                    if (TryParseDecimalOrHex(data, i, j - i, out int len))
                    {
                        int textStart = j + 1;
                        int textEnd = textStart + len;
                        if (len >= 0 && textEnd <= end)
                        {
                            bool printable = true;
                            for (int k = 0; k < len; k++)
                            {
                                if (!IsPrintable(data[textStart + k])) { printable = false; break; }
                            }

                            var run = new BlTextStyleRun
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
                                run.ForeColor = new BlLegacyColor(currentStyle.ColorIndex, currentStyle.ColorIndex, currentStyle.ColorIndex);
                                run.FontSize = currentStyle.FontSize == 0 ? fontSize : currentStyle.FontSize;
                                run.Bold = currentStyle.Bold;
                                run.Italic = currentStyle.Italic;
                                run.Underline = currentStyle.Underline;
                            }

                            if (!printable)
                            {
                                i = textEnd;
                                if (i < end && (data[i] == 0x00 || data[i] == 0x03)) i++;
                                continue;
                            }

                            string text = Encoding.Latin1.GetString(data, textStart, len);
                            run.Text = text;
                            textBuilder.Append(text);
                            doc.Runs.Add(run);

                            i = textEnd;
                            if (i < end && (data[i] == 0x00 || data[i] == 0x03)) i++;
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
            doc.Runs[0].ForeColor = new BlLegacyColor(style.ColorIndex, style.ColorIndex, style.ColorIndex);
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
        style.Locked = !style.EditableField;
    }

    private static void ApplyAlignmentFlags(byte b, XmedStyleDeclaration style)
    {
        style.WrapOff = (b & 0x08) != 0;
        style.HasTabs = (b & 0x10) != 0;
        style.AlignmentFromFlags = DecodeAlignmentFromFlags(b);
        if (!TryDecodeAlignment(b, out var alignment))
        {
            alignment = style.AlignmentFromFlags;
        }

        style.Alignment = alignment;
    }

    private static BlXmedAlignment DecodeAlignmentFromFlags(byte value) => (value & 0x03) switch
    {
        0x00 => BlXmedAlignment.Center,
        0x01 => BlXmedAlignment.Right,
        0x02 => BlXmedAlignment.Left,
        0x03 => BlXmedAlignment.Justify,
        _ => BlXmedAlignment.Center
    };

    private static bool TryDecodeAlignment(byte b, out BlXmedAlignment alignment)
    {
        switch (b)
        {
            case 0x1A:
            case 0x3B:
                alignment = BlXmedAlignment.Left;
                return true;
            case 0x15:
            case 0x77:
                alignment = BlXmedAlignment.Right;
                return true;
            case 0x3F:
            case 0x18:
                alignment = BlXmedAlignment.Center;
                return true;
            default:
                alignment = DecodeAlignmentFromFlags(b);
                return true;
        }
    }

    private static byte DetectAlignmentCode(byte[] data, int start, int end)
    {
        if (data.Length == 0)
        {
            return 0;
        }

        if (start < 0)
        {
            start = 0;
        }

        if (end > data.Length)
        {
            end = data.Length;
        }

        if (start >= end)
        {
            return 0;
        }

        for (int i = start; i < end; i++)
        {
            if (data[i] != 0x02)
            {
                continue;
            }

            int j = i + 1;
            int digits = 0;
            int value = 0;

            while (j < end && TryDecodeHexNibble(data[j], out int nibble))
            {
                if (digits < 2)
                {
                    value = (value << 4) | nibble;
                }

                j++;
                digits++;

                if (digits >= 2)
                {
                    break;
                }
            }

            if (digits == 2 && (j >= end || !IsHexDigit(data[j])))
            {
                byte candidate = (byte)value;
                if (candidate == 0x3B || candidate == 0x3F || candidate == 0x77)
                {
                    return candidate;
                }
            }
        }

        return 0;
    }

    private static bool TryReadAsciiHexUInt16(byte[] data, int offset, out ushort value)
    {
        value = 0;
        if (offset < 0 || offset + 2 > data.Length)
        {
            return false;
        }

        byte low = data[offset];
        byte high = data[offset + 1];

        if (TryDecodeHexNibble(high, out int hi) && TryDecodeHexNibble(low, out int lo))
        {
            value = (ushort)((hi << 4) | lo);
            return true;
        }

        return false;
    }

    private static bool TryDecodeHexNibble(byte b, out int value)
    {
        if (b >= (byte)'0' && b <= (byte)'9')
        {
            value = b - '0';
            return true;
        }

        if (b >= (byte)'A' && b <= (byte)'F')
        {
            value = 10 + (b - 'A');
            return true;
        }

        if (b >= (byte)'a' && b <= (byte)'f')
        {
            value = 10 + (b - 'a');
            return true;
        }

        value = 0;
        return false;
    }

    private static bool IsHexDigit(byte b) =>
        (b >= (byte)'0' && b <= (byte)'9') ||
        (b >= (byte)'A' && b <= (byte)'F') ||
        (b >= (byte)'a' && b <= (byte)'f');

    private static bool IsAllDecimalDigits(byte[] data, int offset, int length)
    {
        if (length <= 0 || offset < 0 || offset + length > data.Length)
        {
            return false;
        }

        for (int k = 0; k < length; k++)
        {
            if (!IsDigit(data[offset + k]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryParseDecimalOrHex(byte[] data, int offset, int length, out int value)
    {
        value = 0;
        if (length <= 0 || offset < 0 || offset + length > data.Length)
        {
            return false;
        }

        if (IsAllDecimalDigits(data, offset, length))
        {
            if (int.TryParse(Encoding.ASCII.GetString(data, offset, length), out value))
            {
                return true;
            }
        }

        for (int k = 0; k < length; k++)
        {
            if (!IsHexDigit(data[offset + k]))
            {
                return false;
            }
        }

        return int.TryParse(Encoding.ASCII.GetString(data, offset, length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
    }

    private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

    private static bool IsPrintable(byte b) 
    {
        // Allow standard ASCII printable characters (32-126)
        if (b >= 32 && b <= 126) return true;
        
        // Allow common whitespace characters that should be preserved in text
        if (b == 9 || b == 10 || b == 13) return true; // Tab, LF, CR
        
        // Allow extended ASCII characters (128-255) for international text
        if (b >= 128) return true;
        
        // Reject control characters (0-8, 11-12, 14-31)
        return false;
    }
}

