namespace BlingoEngine.IO.Legacy.Afterburner;

/// <summary>
/// Holds state derived from the <c>FGEI</c> chunk. The <see cref="BodyOffset"/> property marks the absolute stream position of
/// the Afterburner data section so resource offsets can be resolved relative to the movie.
/// </summary>
internal sealed class BlAfterburnerState
{
    public BlAfterburnerState(long bodyOffset)
    {
        BodyOffset = bodyOffset;
    }

    /// <summary>
    /// Gets the absolute offset, in bytes, where Afterburner resource bodies begin.
    /// </summary>
    public long BodyOffset { get; }
}
