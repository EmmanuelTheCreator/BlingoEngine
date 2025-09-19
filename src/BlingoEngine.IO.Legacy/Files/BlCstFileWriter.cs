using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Writes <c>.cst</c> cast libraries with the same <c>XFIR</c>/<c>MV93</c> header used by classic Director
/// authoring movies so the resulting archive can be imported by legacy tooling.
/// </summary>
public sealed class BlCstFileWriter : BlLegacyFileWriterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlCstFileWriter"/> class.
    /// </summary>
    public BlCstFileWriter()
        : base(
            BlTag.MV93,
            isBigEndian: false,
            BlLegacyFormatConstants.ClassicMapVersion,
            BlLegacyFormatConstants.Director101ArchiveVersion)
    {
    }
}
