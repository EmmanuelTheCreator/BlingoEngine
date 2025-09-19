using System;

namespace BlingoEngine.IO.Legacy.Fields;

/// <summary>
/// Describes the payload format for editable field cast members. Older movies store the text
/// content inside <c>STXT</c> resources while newer versions may upgrade to styled containers.
/// The value lets consumers decide how to decode the <see cref="BlLegacyField.Bytes"/> buffer.
/// </summary>
public enum BlLegacyFieldFormatKind
{
    /// <summary>
    /// Format could not be determined from the resource metadata.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Plain <c>STXT</c> bytes that hold the field text content.
    /// </summary>
    Stxt,

    /// <summary>
    /// Styled text stored in an <c>XMED</c> chunk. The reader does not yet decode the structure but
    /// exposes the raw bytes for experimentation.
    /// </summary>
    Xmed
}
