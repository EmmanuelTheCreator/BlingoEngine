namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Reader for <c>.cst</c> library cast archives that share the same 12-byte header and chunked structure as authoring movies.
/// </summary>
public sealed class BlCstFile : BlLegacyFileResourceBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlCstFile"/> class.
    /// </summary>
    public BlCstFile(ReaderContext context)
        : base(context)
    {
    }
}
