using System;

namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents the logical format flags decoded from the byte-oriented headers inside a Director container.
/// The <c>RIFX/XFIR</c> signature consumes bytes 0x00-0x03 of the movie stream, the declared payload length
/// occupies bytes 0x04-0x07, and the four-character codec marker sits at bytes 0x08-0x0B. Subsequent
/// <c>imap</c> bytes expose a 32-bit archive version that maps to Director 4 (0x00000000), Director 5 (0x000004C1),
/// Director 6 (0x000004C7), Director 8.5 (0x00000708), and Director 10 (0x00000742). These markers provide the
/// version metadata necessary to choose the correct resource map interpretation without referencing external tools.
/// </summary>
public sealed class BlDataFormat
{
    private uint _archiveVersion;

    /// <summary>
    /// Gets or sets the codec tag decoded from bytes 0x08-0x0B of the movie header (e.g. <c>MV93</c>, <c>MC95</c>, <c>FGDM</c>).
    /// </summary>
    public BlRifxCodec Codec { get; set; } = BlRifxCodec.Unknown;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>RIFX</c> signature flagged big-endian byte ordering.
    /// </summary>
    public bool IsBigEndian { get; set; }

    /// <summary>
    /// Gets a value indicating whether the codec bytes identify a compressed Afterburner movie (<c>FGDM</c> or <c>FGDC</c>).
    /// </summary>
    public bool IsAfterburner => Codec is BlRifxCodec.FGDM or BlRifxCodec.FGDC;

    /// <summary>
    /// Gets or sets the Director archive version read from the 32-bit value in the <c>imap</c> control block.
    /// Updating this property recalculates <see cref="DirectorVersion"/> and <see cref="DirectorVersionLabel"/>.
    /// </summary>
    public uint ArchiveVersion
    {
        get => _archiveVersion;
        set
        {
            _archiveVersion = value;
            DirectorVersion = MapDirectorVersion(value);
            DirectorVersionLabel = DirectorVersion == 0
                ? $"Unknown (0x{_archiveVersion:X})"
                : $"Director {DirectorVersion}";
        }
    }

    /// <summary>
    /// Gets or sets the memory-map version recorded alongside the <c>imap</c> bytes.
    /// </summary>
    public uint MapVersion { get; set; }

    /// <summary>
    /// Gets the inferred Director major version derived from <see cref="ArchiveVersion"/>.
    /// </summary>
    public int DirectorVersion { get; private set; }

    /// <summary>
    /// Gets a formatted label describing the Director release that emitted the observed archive version bytes.
    /// </summary>
    public string DirectorVersionLabel { get; private set; } = "Unknown";

    /// <summary>
    /// Gets or sets the textual Afterburner build identifier recorded in the <c>Fver</c> chunk.
    /// </summary>
    public string AfterburnerVersion { get; set; } = string.Empty;

    private static int MapDirectorVersion(uint rawVersion) => rawVersion switch
    {
        0x00000000 => 4,
        0x000004C1 => 5,
        0x000004C7 => 6,
        0x00000708 => 8,
        0x00000742 => 10,
        _ => 0
    };
}
