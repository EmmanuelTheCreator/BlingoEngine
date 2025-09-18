namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Describes how a resource payload is stored inside the container.
/// </summary>
public enum BlResourceStorageKind
{
    /// <summary>
    /// Resource data is stored as a classic chunk referenced by the <c>mmap</c> table.
    /// </summary>
    ClassicChunk,

    /// <summary>
    /// Resource data is referenced through Afterburner metadata (<c>ABMP</c>/<c>FGEI</c>).
    /// </summary>
    AfterburnerSegment
}
