using System;
using System.Buffers.Binary;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts;

/// <summary>
/// Provides helpers for decoding the plain text payloads found in legacy <c>STXT</c> resources.
/// Director typically prefixes the text bytes with a big-endian length field, but some movies omit
/// it, so the decoder falls back to consuming the entire buffer when the prefix is missing or
/// inconsistent.
/// </summary>
public static class BlLegacyPlainTextDecoder
{
    /// <summary>
    /// Converts the supplied <c>STXT</c> payload into a UTF-8 string while tolerating truncated or
    /// padded buffers that appear in early projector versions.
    /// </summary>
    /// <param name="data">Raw bytes copied from the <c>STXT</c> resource.</param>
    /// <returns>The decoded string with trailing null characters trimmed.</returns>
    public static string Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 2)
        {
            var declaredLength = BinaryPrimitives.ReadUInt16BigEndian(data);
            if (declaredLength > 0 && declaredLength <= data.Length - 2)
            {
                return Encoding.UTF8.GetString(data.Slice(2, declaredLength)).TrimEnd('\0');
            }
        }

        return Encoding.UTF8.GetString(data).TrimEnd('\0');
    }
}
