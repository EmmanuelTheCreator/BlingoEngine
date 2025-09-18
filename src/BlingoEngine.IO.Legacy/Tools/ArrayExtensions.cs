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
    }
}
