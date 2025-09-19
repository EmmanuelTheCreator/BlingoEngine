using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Writes <c>.dct</c> protected movies using the <c>XFIR</c>/<c>MC95</c> header observed in Director runtime
/// templates while sharing the classic map layout implementation.
/// </summary>
public sealed class BlDctFileWriter : BlLegacyFileWriterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDctFileWriter"/> class.
    /// </summary>
    public BlDctFileWriter()
        : base(
            BlTag.MC95,
            isBigEndian: false,
            BlLegacyFormatConstants.ClassicMapVersion,
            BlLegacyFormatConstants.Director101ArchiveVersion)
    {
    }
}
