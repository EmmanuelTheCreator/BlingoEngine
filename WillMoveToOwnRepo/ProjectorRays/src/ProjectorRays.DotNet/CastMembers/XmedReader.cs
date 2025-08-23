using System;
using System.Collections.Generic;
using System.Text;
using ProjectorRays.Common;
using static ProjectorRays.CastMembers.XmedChunkParser;

namespace ProjectorRays.CastMembers;

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
}

/// <summary>Simple style declaration extracted from XMED.</summary>
public sealed class XmedStyleDeclaration
{
    public string Id { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty;
    public byte ColorIndex { get; set; } = 0;
}

/// <summary>
/// Basic reader for Director XMED chunks.  This is a best-effort
/// implementation based on observed sample files.  The format is not fully
/// documented, but this reader extracts plain text and the most obvious style
/// information so that callers can experiment with the data.
/// </summary>
public static class XmedReader
{
    public static XmedDocument Read(BufferView view)
    {
        var data = view.Data;
        int start = view.Offset;
        int end = start + view.Size;

        if (view.Size < 4 || Encoding.ASCII.GetString(data, start, 4) != "DEMX")
            throw new InvalidDataException("Invalid XMED chunk header");

        var doc = new XmedDocument();
        var textBuilder = new StringBuilder();

        // Parse sequentially after the header.  The first 4 bytes are "DEMX" so
        // begin scanning at offset+4.
        int i = start + 4;
        XmedStyleDeclaration? currentStyle = null;

        while (i < end)
        {
            byte b = data[i];

            // Pattern: "40," + color + font name + NUL
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
                        ColorIndex = color
                    };
                    doc.Styles.Add(currentStyle);
                }

                i = j;
                if (i < end && data[i] == 0) i++;
                continue;
            }

            // Pattern: digits followed by ',' and the text length.
            if (IsDigit(b))
            {
                int j = i;
                while (j < end && IsDigit(data[j])) j++;
                if (j < end && data[j] == 0x2C) // comma
                {
                    string num = Encoding.ASCII.GetString(data, i, j - i);
                    if (int.TryParse(num, out int len))
                    {
                        int textStart = j + 1;
                        int available = Math.Min(len, end - textStart);
                        string text = Encoding.Latin1.GetString(data, textStart, available);

                        var run = new TextStyleRun
                        {
                            Text = text,
                            Length = available,
                            Start = textBuilder.Length
                        };

                        if (currentStyle != null)
                        {
                            run.FontName = currentStyle.FontName;
                            run.ForeColor = new RayColor(currentStyle.ColorIndex, currentStyle.ColorIndex, currentStyle.ColorIndex);
                        }

                        doc.Runs.Add(run);
                        textBuilder.Append(text);

                        i = textStart + available;
                        if (i < end && (data[i] == 0x00 || data[i] == 0x03))
                            i++;
                        continue;
                    }
                }
            }

            i++;
        }

        doc.Text = textBuilder.ToString();
        return doc;
    }

    private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

    private static bool IsPrintable(byte b) => b >= 32 && b <= 126;
}

