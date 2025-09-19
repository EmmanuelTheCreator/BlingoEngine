using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Writes <c>.dxr</c> protected movies using the <c>XFIR</c>/<c>MC95</c> header observed in Director runtime
/// templates while sharing the classic map layout implementation.
/// </summary>
public sealed class BlDxrFileWriter : BlLegacyFileWriterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDxrFileWriter"/> class.
    /// </summary>
    public BlDxrFileWriter(BlLegacyDirectorVersion directorVersion = BlLegacyDirectorVersion.Latest)
        : base(
            BlTag.MC95,
            isBigEndian: false,
            directorVersion)
    {
    }
}
