using System;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Text;

namespace ProjectorRays.CastMembers;

internal static class XmedTextParsingHelpers
{
    public static byte[] ReadAll(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
        {
            long position = stream.Position;
            stream.Position = 0;
            try
            {
                return ReadAllImpl(stream);
            }
            finally
            {
                stream.Position = position;
            }
        }

        return ReadAllImpl(stream);
    }

    private static byte[] ReadAllImpl(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public static uint ReadU32LE(byte[] buffer, int offset)
    {
        if (offset < 0 || offset + 4 > buffer.Length)
        {
            return 0u;
        }

        return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
    }

    public static byte ReadU8(byte[] buffer, int offset)
    {
        if (offset < 0 || offset >= buffer.Length)
        {
            return 0;
        }

        return buffer[offset];
    }

    public static bool IsDigitOrHex(byte value)
    {
        return (value >= (byte)'0' && value <= (byte)'9') ||
               (value >= (byte)'A' && value <= (byte)'F') ||
               (value >= (byte)'a' && value <= (byte)'f');
    }

    public static bool IsPrintable(byte value)
    {
        return (value >= 32 && value <= 126) ||
               value == 9 ||
               value == 10 ||
               value == 13 ||
               value >= 128;
    }

    public static bool IsDigits(ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return false;
        }

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] < (byte)'0' || span[i] > (byte)'9')
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsDigits(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return false;
        }

        for (int i = 0; i < span.Length; i++)
        {
            if (!char.IsDigit(span[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsAsciiHexOrDigits(ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return false;
        }

        for (int i = 0; i < span.Length; i++)
        {
            var value = span[i];
            if (!((value >= (byte)'0' && value <= (byte)'9') ||
                  (value >= (byte)'A' && value <= (byte)'F') ||
                  (value >= (byte)'a' && value <= (byte)'f')))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsAsciiHexOrDigitString(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryParseUInt32Decimal(ReadOnlySpan<byte> span, out uint value)
    {
        value = 0;
        if (!IsDigits(span))
        {
            return false;
        }

        var text = Encoding.ASCII.GetString(span);
        return uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseUInt32Hex(ReadOnlySpan<byte> span, out uint value)
    {
        value = 0;
        var text = Encoding.ASCII.GetString(span);
        return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
    }

    public static long ParseU64(ReadOnlySpan<char> span)
    {
        return long.Parse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    public static int ParseInt(ReadOnlySpan<byte> span)
    {
        var text = Encoding.ASCII.GetString(span);
        return int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static int IndexOfSequence(byte[] haystack, int start, ReadOnlySpan<byte> needle)
    {
        if (needle.Length == 0)
        {
            return start;
        }

        for (int i = Math.Max(start, 0); i <= haystack.Length - needle.Length; i++)
        {
            if (haystack[i] != needle[0])
            {
                continue;
            }

            int k = 1;
            for (; k < needle.Length; k++)
            {
                if (haystack[i + k] != needle[k])
                {
                    break;
                }
            }

            if (k == needle.Length)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool IsRunType(string type)
    {
        return type is "0004" or "0005" or "0006" or "0007";
    }
}
