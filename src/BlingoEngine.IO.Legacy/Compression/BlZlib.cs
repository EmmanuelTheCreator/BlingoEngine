using System;
using System.IO;
using System.IO.Compression;

namespace BlingoEngine.IO.Legacy.Compression;

public static class BlZlib
{
    public static byte[] Decompress(byte[] data, int? expectedLength = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Decompress((ReadOnlyMemory<byte>)data, expectedLength);
    }

    public static byte[] Decompress(ReadOnlyMemory<byte> data, int? expectedLength = null)
    {
        using var source = new MemoryStream(data.ToArray(), writable: false);
        using var stream = new ZLibStream(source, CompressionMode.Decompress);

        if (expectedLength is { } length && length > 0)
        {
            var buffer = new byte[length];
            var offset = 0;
            while (offset < length)
            {
                var read = stream.Read(buffer, offset, length - offset);
                if (read == 0)
                {
                    break;
                }

                offset += read;
            }

            if (offset != length)
            {
                Array.Resize(ref buffer, offset);
            }

            return buffer;
        }

        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }
}
