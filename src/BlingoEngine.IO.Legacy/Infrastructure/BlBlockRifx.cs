using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using BlingoEngine.IO.Legacy;

namespace BlingoEngine.IO.Legacy.Infrastructure;

/// <summary>
/// Locates the 12-byte <c>RIFX/XFIR</c> movie header inside executable or projector shells. Projectors prefix the movie with
/// a four-byte marker (e.g. <c>PJ93</c>) followed by a 32-bit offset. When neither case matches, the helper scans the byte stream
/// for the signature to accommodate self-extracting archives that embed the movie deeper within the file.
/// </summary>
internal static class BlBlockRifx
{
    private static readonly string[] ProjectorMarkers = { "PJ93", "PJ95", "PJ00", "PJ01" };

    public static long Locate(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var original = stream.Position;
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            Span<byte> header = stackalloc byte[4];
            if (!TryFill(stream, header))
            {
                throw new InvalidDataException("Stream is too small to contain a RIFX header.");
            }

            if (IsMagic(header))
            {
                return 0;
            }

            var marker = Encoding.ASCII.GetString(header);
            if (IsProjectorMarker(marker))
            {
                Span<byte> offsetBytes = stackalloc byte[4];
                if (!TryFill(stream, offsetBytes))
                {
                    throw new InvalidDataException("Projector header is truncated.");
                }

                var littleEndianOffset = BinaryPrimitives.ReadUInt32LittleEndian(offsetBytes);
                if (IsValidOffset(stream, littleEndianOffset))
                {
                    return littleEndianOffset;
                }

                var bigEndianOffset = BinaryPrimitives.ReadUInt32BigEndian(offsetBytes);
                if (IsValidOffset(stream, bigEndianOffset))
                {
                    return bigEndianOffset;
                }

                throw new InvalidDataException("Projector header does not contain a valid movie offset.");
            }

            var scanned = ScanForMagic(stream);
            if (scanned >= 0)
            {
                return scanned;
            }

            throw new InvalidDataException("Unable to locate RIFX magic in the provided stream.");
        }
        finally
        {
            stream.Seek(original, SeekOrigin.Begin);
        }
    }

    private static bool TryFill(Stream stream, Span<byte> buffer)
    {
        var filled = 0;
        while (filled < buffer.Length)
        {
            var read = stream.Read(buffer[filled..]);
            if (read == 0)
            {
                return false;
            }

            filled += read;
        }

        return true;
    }

    private static bool IsProjectorMarker(string text)
    {
        foreach (var marker in ProjectorMarkers)
        {
            if (string.Equals(marker, text, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsMagic(ReadOnlySpan<byte> bytes)
    {
        return BlTag.Equals(bytes, 0, BlTag.RIFX) || BlTag.Equals(bytes, 0, BlTag.XFIR);
    }

    private static bool IsValidOffset(Stream stream, uint offset)
    {
        if (offset >= stream.Length)
        {
            return false;
        }

        var current = stream.Position;
        try
        {
            stream.Seek(offset, SeekOrigin.Begin);
            Span<byte> buffer = stackalloc byte[4];
            if (!TryFill(stream, buffer))
            {
                return false;
            }

            return IsMagic(buffer);
        }
        finally
        {
            stream.Seek(current, SeekOrigin.Begin);
        }
    }

    private static long ScanForMagic(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        Span<byte> window = stackalloc byte[4];
        if (!TryFill(stream, window))
        {
            return -1;
        }

        if (IsMagic(window))
        {
            return 0;
        }

        long offset = 0;
        while (true)
        {
            var next = stream.ReadByte();
            if (next < 0)
            {
                return -1;
            }

            window[0] = window[1];
            window[1] = window[2];
            window[2] = window[3];
            window[3] = (byte)next;
            offset++;

            if (IsMagic(window))
            {
                return offset;
            }
        }
    }
}
