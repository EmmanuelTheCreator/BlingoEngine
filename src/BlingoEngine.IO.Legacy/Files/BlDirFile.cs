using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Reader for <c>.dir</c> authoring files that rely on the 12-byte movie header and classic <c>imap</c>/<c>mmap</c> metadata.
/// </summary>
public sealed class BlDirFile : BlLegacyFileResourceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDirFile"/> class.
    /// </summary>
    public BlDirFile(ReaderContext context)
        : base(context)
    {
    }
}
