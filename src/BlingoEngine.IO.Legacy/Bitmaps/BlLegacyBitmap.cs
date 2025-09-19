using System;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Represents the decoded payload of a legacy Director bitmap resource. The loader exposes a
/// lightweight <see cref="BlLegacyBitmapFormatKind"/> classification alongside the raw bytes so
/// higher layers can decide how to persist or transcode the image data.
/// </summary>
public sealed class BlLegacyBitmap
{
    /// <summary>
    /// Gets the resource identifier associated with the bitmap entry in the resource table.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the detected bitmap container format.
    /// </summary>
    public BlLegacyBitmapFormatKind Format { get; }

    /// <summary>
    /// Gets the raw bytes streamed from the bitmap resource.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyBitmap"/> class.
    /// </summary>
    public BlLegacyBitmap(int resourceId, BlLegacyBitmapFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
