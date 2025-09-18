namespace BlingoEngine.IO.Legacy.Compression;

/// <summary>
/// Enumerates compression schemes referenced by the 16-byte identifiers stored in the <c>Fcdr</c> block.
/// </summary>
public enum BlCompressionKind
{
    None = 0,
    Zlib,
    Sound,
    FontMap,
    Unknown
}
