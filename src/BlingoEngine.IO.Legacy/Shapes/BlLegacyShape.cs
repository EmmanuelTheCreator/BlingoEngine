using System;

namespace BlingoEngine.IO.Legacy.Shapes;

/// <summary>
/// Represents the raw payload stored inside a legacy <c>CASt</c> record for shape cast members.
/// The loader keeps the 17-byte QuickDraw structure untouched so callers can translate the
/// geometry, ink, and colour values according to their needs.
/// </summary>
internal sealed class BlLegacyShape
{
    /// <summary>
    /// Gets the resource identifier associated with the <c>CASt</c> chunk that supplied the shape data.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the colour interpretation that applies to the stored bytes. Director 2–3 records keep
    /// signed QuickDraw colour components while Director 4–10 switch to unsigned values. Releases
    /// beyond Director 10 are flagged as placeholders until their layout is confirmed.
    /// </summary>
    public BlLegacyShapeFormatKind Format { get; }

    /// <summary>
    /// Gets the raw shape record extracted from the cast data. The buffer stores the fields described
    /// in docs/LegacyShapeRecords.md using the following offsets:
    /// <list type="bullet">
    /// <item><description><c>0x00-0x01</c> – QuickDraw shape enumeration.</description></item>
    /// <item><description><c>0x02-0x09</c> – Initial bounding rectangle (top, left, bottom, right).</description></item>
    /// <item><description><c>0x0A-0x0B</c> – Fill pattern identifier.</description></item>
    /// <item><description><c>0x0C</c> – Foreground colour byte.</description></item>
    /// <item><description><c>0x0D</c> – Background colour byte.</description></item>
    /// <item><description><c>0x0E</c> – Fill and ink flags.</description></item>
    /// <item><description><c>0x0F</c> – Pen thickness.</description></item>
    /// <item><description><c>0x10</c> – Line direction for patterned strokes.</description></item>
    /// </list>
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyShape"/> class.
    /// </summary>
    public BlLegacyShape(int resourceId, BlLegacyShapeFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
