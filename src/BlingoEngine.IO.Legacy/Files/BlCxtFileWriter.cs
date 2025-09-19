using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Writes <c>.cxt</c> protected cast libraries using the <c>XFIR</c>/<c>MC95</c> header so the archive is
/// treated as read-only by Director while reusing the classic map layout implementation.
/// </summary>
public sealed class BlCxtFileWriter : BlLegacyFileWriterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlCxtFileWriter"/> class.
    /// </summary>
    public BlCxtFileWriter(BlLegacyDirectorVersion directorVersion = BlLegacyDirectorVersion.Latest)
        : base(
            BlTag.MC95,
            isBigEndian: false,
            directorVersion)
    {
    }
}
