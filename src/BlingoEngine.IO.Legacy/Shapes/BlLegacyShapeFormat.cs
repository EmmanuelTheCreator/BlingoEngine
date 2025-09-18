using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Shapes;

/// <summary>
/// Describes how the colour bytes stored in a legacy shape record should be interpreted.
/// Director 2–3 write signed QuickDraw components, whereas Director 4–10 store them as
/// unsigned values. Later releases beyond Director 10 only expose placeholder data until
/// the structure is documented.
/// </summary>
internal enum BlLegacyShapeFormatKind
{
    /// <summary>
    /// The movie metadata did not provide enough information to classify the record.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Director 2–3 record that encodes the QuickDraw colours as signed bytes which
    /// must be normalised back into the 0–255 range. See docs/LegacyShapeRecords.md.
    /// </summary>
    Director2To3SignedColors,

    /// <summary>
    /// Director 4–10 record that stores unsigned QuickDraw colour components.
    /// The 17-byte payload layout matches the table documented in
    /// docs/LegacyShapeRecords.md.
    /// </summary>
    Director4To10UnsignedColors,

    /// <summary>
    /// Director 11+ placeholder. Projectors in this range did not populate the
    /// shape data according to available documentation, so the loader preserves the
    /// bytes for auditing but treats the record as experimental.
    /// </summary>
    Director11PlusPlaceholder
}

/// <summary>
/// Provides helpers for combining parsed shape metadata with the surrounding movie format
/// so the caller receives the most accurate colour interpretation possible.
/// </summary>
internal static class BlLegacyShapeFormat
{
    private const uint Director11ArchiveMarker = 0x00001100;

    public static BlLegacyShapeFormatKind Resolve(BlDataFormat? format, BlLegacyShapeFormatKind parsedFormat)
    {
        if (format is null)
        {
            return parsedFormat;
        }

        if (format.ArchiveVersion >= Director11ArchiveMarker || format.DirectorVersion >= 11)
        {
            return BlLegacyShapeFormatKind.Director11PlusPlaceholder;
        }

        if (parsedFormat != BlLegacyShapeFormatKind.Unknown)
        {
            return parsedFormat;
        }

        if (format.DirectorVersion == 0)
        {
            return BlLegacyShapeFormatKind.Director2To3SignedColors;
        }

        if (format.DirectorVersion >= 4)
        {
            return BlLegacyShapeFormatKind.Director4To10UnsignedColors;
        }

        return parsedFormat;
    }
}
