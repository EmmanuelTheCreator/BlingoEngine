using System;

namespace BlingoEngine.IO.Legacy.Compression;

/// <summary>
/// Exposes known 16-byte identifiers used in the <c>Fcdr</c> compression table. These values correspond to zlib streams,
/// uncompressed payloads, sound codecs, and font map resources observed in Director files.
/// </summary>
public static class BlCompressionFormat
{
    public static ReadOnlyMemory<byte> NullId { get; } = new byte[]
    {
        0xAC, 0x99, 0x98, 0x2E, 0x00, 0x5D, 0x0D, 0x50, 0x00, 0x00, 0x08, 0x00, 0x07, 0x37, 0x7A, 0x34
    };

    public static ReadOnlyMemory<byte> ZlibId { get; } = new byte[]
    {
        0xAC, 0x99, 0xE9, 0x04, 0x00, 0x70, 0x0B, 0x36, 0x00, 0x00, 0x08, 0x00, 0x07, 0x37, 0x7A, 0x34
    };

    public static ReadOnlyMemory<byte> SoundId { get; } = new byte[]
    {
        0x72, 0x04, 0xA8, 0x89, 0xAF, 0xD0, 0x11, 0xCF, 0xA2, 0x22, 0x00, 0xA0, 0x24, 0x53, 0x44, 0x4C
    };

    public static ReadOnlyMemory<byte> FontMapId { get; } = new byte[]
    {
        0x8A, 0x46, 0x79, 0xA1, 0x37, 0x20, 0x11, 0xD0, 0x92, 0x23, 0x00, 0xA0, 0xC9, 0x08, 0x68, 0xB1
    };

    /// <summary>
    /// Resolves the compression kind using the 16-byte identifier and the descriptor name read from <c>Fcdr</c>.
    /// </summary>
    public static BlCompressionKind Resolve(ReadOnlySpan<byte> identifier, string name)
    {
        if (identifier.SequenceEqual(NullId.Span))
            return BlCompressionKind.None;

        if (identifier.SequenceEqual(ZlibId.Span) || name.Contains("zlib", StringComparison.OrdinalIgnoreCase))
            return BlCompressionKind.Zlib;

        if (identifier.SequenceEqual(SoundId.Span) || name.Contains("sound", StringComparison.OrdinalIgnoreCase))
            return BlCompressionKind.Sound;

        if (identifier.SequenceEqual(FontMapId.Span) || name.Contains("font", StringComparison.OrdinalIgnoreCase))
            return BlCompressionKind.FontMap;

        return BlCompressionKind.Unknown;
    }
}
