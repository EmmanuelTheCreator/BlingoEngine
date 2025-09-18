namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Reader for <c>.dcr</c>/<c>.dxr</c> runtime movies, which frequently embed Afterburner (<c>FGDM/FGDC</c>) metadata atop the
/// 12-byte RIFX/XFIR header and control bytes.
/// </summary>
public sealed class BlDcrFile : BlLegacyFileResourceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlDcrFile"/> class.
    /// </summary>
    public BlDcrFile(ReaderContext context)
        : base(context)
    {
    }
}
