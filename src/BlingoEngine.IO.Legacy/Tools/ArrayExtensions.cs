using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace BlingoEngine.IO.Legacy.Tools
{
    public static class ArrayExtensions
    {
        public static string ReadPascalString(this Span<byte> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }

            int length = span[0];
            if (length <= 0 || length >= span.Length)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(span.Slice(1, length));
        }
        public static string ReadPascalString(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }

            int length = span[0];
            if (length <= 0 || length >= span.Length)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(span.Slice(1, length));
        }

        public static string ScanForPascalString(this byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                int length = data[i];
                if (length <= 0 || i + 1 + length > data.Length)
                {
                    continue;
                }

                bool ascii = true;
                for (var j = 0; j < length; j++)
                {
                    var value = data[i + 1 + j];
                    if (value < 0x20 || value > 0x7E)
                    {
                        ascii = false;
                        break;
                    }
                }

                if (ascii)
                {
                    return Encoding.UTF8.GetString(data, i + 1, length);
                }
            }

            return string.Empty;
        }

        public static string ExtractName(this byte[] infoData)
        {
            if (infoData.Length < 10)
            {
                return infoData.ScanForPascalString();
            }

            using var memory = new MemoryStream(infoData, writable: false);
            var reader = new BlStreamReader(memory)
            {
                Endianness = BlEndianness.BigEndian
            };

            var _ = reader.ReadUInt32();
            var entryCount = reader.ReadUInt16();
            var itemsLength = reader.ReadUInt32();

            if (entryCount == 0)
            {
                return infoData.ScanForPascalString();
            }

            var offsets = new uint[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                if (reader.Position > infoData.Length - 4)
                {
                    return infoData.ScanForPascalString();
                }

                offsets[i] = reader.ReadUInt32();
            }

            var tableEnd = (int)reader.Position;
            var available = infoData.Length - tableEnd;
            if (available <= 0)
            {
                return infoData.ScanForPascalString();
            }

            var itemsLimit = (int)Math.Min(itemsLength, (uint)available);
            if (itemsLimit <= 0)
            {
                return infoData.ScanForPascalString();
            }

            if (entryCount >= 2)
            {
                var start = (int)Math.Min(offsets[1], (uint)itemsLimit);
                var nextOffset = entryCount > 2 ? offsets[2] : itemsLength;
                var end = (int)Math.Min(nextOffset, (uint)itemsLimit);
                var length = end - start;
                if (start >= 0 && length > 0 && start + length <= itemsLimit)
                {
                    var itemsSpan = infoData.AsSpan(tableEnd, itemsLimit);
                    Span<byte> itemSpan = itemsSpan.Slice(start, length);
                    var name = itemSpan.ReadPascalString();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
            }

            return infoData.ScanForPascalString();
        }

        public static string ToHexString(this ReadOnlySpan<byte> data, int bytesPerLine = 16, bool includeAddresses = false)
        {
            if (bytesPerLine <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesPerLine));
            }

            if (data.IsEmpty)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
            {
                if (i % bytesPerLine == 0)
                {
                    if (i > 0)
                    {
                        builder.AppendLine();
                    }

                    if (includeAddresses)
                    {
                        builder.Append($"0x{i:X8}: ");
                    }
                }
                else
                {
                    builder.Append(' ');
                }

                builder.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Formats bytes as a hexadecimal string, optionally prefixing addresses on each line.
        /// </summary>
        /// <param name="data">Byte array to format.</param>
        /// <param name="bytesPerLine">Number of bytes shown per output line.</param>
        /// <param name="includeAddresses">Whether to prefix each line with the starting offset.</param>
        /// <returns>Formatted hexadecimal text for the supplied bytes.</returns>
        public static string ToHexString(this byte[] data, int bytesPerLine = 16, bool includeAddresses = false)
        {
            ArgumentNullException.ThrowIfNull(data);
            return ToHexString(data.AsSpan(), bytesPerLine, includeAddresses);
        }

        public static uint ReadUInt32LittleEndian(this byte[] buffer, int offset)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (offset < 0 || offset + 4 > buffer.Length)
            {
                return 0u;
            }

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
        }

        public static byte ReadByteOrDefault(this byte[] buffer, int offset)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (offset < 0 || offset >= buffer.Length)
            {
                return 0;
            }

            return buffer[offset];
        }

        public static bool IsDigitOrHex(this byte value)
        {
            return (value >= (byte)'0' && value <= (byte)'9') ||
                   (value >= (byte)'A' && value <= (byte)'F') ||
                   (value >= (byte)'a' && value <= (byte)'f');
        }

        public static bool IsPrintable(this byte value)
        {
            return (value >= 32 && value <= 126) ||
                   value == 9 ||
                   value == 10 ||
                   value == 13 ||
                   value >= 128;
        }

        public static bool IsDigits(this ReadOnlySpan<byte> span)
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

        public static bool IsDigits(this ReadOnlySpan<char> span)
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

        public static bool IsAsciiHexOrDigits(this ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return false;
            }

            for (int i = 0; i < span.Length; i++)
            {
                var value = span[i];
                if (!value.IsDigitOrHex())
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAsciiHexOrDigitString(this string text)
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

        public static bool TryParseUInt32Decimal(this ReadOnlySpan<byte> span, out uint value)
        {
            value = 0;
            if (!span.IsDigits())
            {
                return false;
            }

            var text = Encoding.ASCII.GetString(span);
            return uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseUInt32Hex(this ReadOnlySpan<byte> span, out uint value)
        {
            value = 0;
            var text = Encoding.ASCII.GetString(span);
            return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        public static long ParseHexInt64(this ReadOnlySpan<char> span)
        {
            return long.Parse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static int ParseInt32(this ReadOnlySpan<byte> span)
        {
            var text = Encoding.ASCII.GetString(span);
            return int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        public static int IndexOfSequence(this byte[] haystack, int start, ReadOnlySpan<byte> needle)
        {
            ArgumentNullException.ThrowIfNull(haystack);

            if (needle.Length == 0)
            {
                return Math.Max(start, 0);
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
    }
}
