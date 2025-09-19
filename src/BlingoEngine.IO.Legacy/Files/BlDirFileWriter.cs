using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Writes classic <c>.dir</c> authoring movies using the little-endian <c>XFIR</c>/<c>MV93</c> header and
/// a Director 10-style map layout.
/// </summary>
public sealed class BlDirFileWriter : BlLegacyFileWriterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDirFileWriter"/> class.
    /// </summary>
    public BlDirFileWriter()
        : base(
            BlTag.MV93,
            isBigEndian: false,
            BlLegacyFormatConstants.ClassicMapVersion,
            BlLegacyFormatConstants.Director101ArchiveVersion)
    {
    }
}
