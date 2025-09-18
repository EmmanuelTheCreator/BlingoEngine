using System;

namespace BlingoEngine.IO.Legacy.Fields;

/// <summary>
/// Represents the raw payload backing an editable field cast member. The structure mirrors the text
/// reader by surfacing the detected format alongside the byte array extracted from the archive so
/// higher layers can decode the content at their own pace.
/// </summary>
internal sealed class BlLegacyField
{
    /// <summary>
    /// Gets the identifier assigned to the field resource in the map table.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the detected field payload format such as <c>STXT</c>.
    /// </summary>
    public BlLegacyFieldFormatKind Format { get; }

    /// <summary>
    /// Gets the raw field bytes copied straight from the resource stream. Historical layouts are
    /// cataloged in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> so consumers can interpret the payload even
    /// when styled chunks have not been fully decoded.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyField"/> class.
    /// </summary>
    public BlLegacyField(int resourceId, BlLegacyFieldFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
