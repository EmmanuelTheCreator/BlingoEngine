using System;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Represents the decoded payload of a legacy Director sound resource. The loader classifies the
/// raw bytes into a lightweight <see cref="BlLegacySoundFormatKind"/> so higher layers can decide how
/// to persist or transcode the data.
/// </summary>
public sealed class BlLegacySound
{
    /// <summary>
    /// Gets the resource identifier associated with the sound entry in the resource table.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the best-effort classification of the sound payload.
    /// </summary>
    public BlLegacySoundFormatKind Format { get; }

    /// <summary>
    /// Gets the raw bytes stored inside the <c>ediM</c> chunk.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacySound"/> class.
    /// </summary>
    public BlLegacySound(int resourceId, BlLegacySoundFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
