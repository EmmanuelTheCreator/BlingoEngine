using System;
using System.Diagnostics.CodeAnalysis;

namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Consolidates version markers shared across the legacy Director readers and writers so the
/// archive metadata remains consistent regardless of which side emits the container bytes.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BlLegacyFormatConstants
{
    /// <summary>
    /// Classic <c>imap</c> tables store a map version of <c>1</c> for Director 4 through Director 11
    /// movies. The writers reuse the same value to align with the reader defaults.
    /// </summary>
    public const uint ClassicMapVersion = 1;

    /// <summary>
    /// Director 4 archive version marker observed in <c>imap</c> payloads.
    /// </summary>
    public const uint Director4ArchiveVersion = 0x00000000;

    /// <summary>
    /// Director 5 archive version marker observed in <c>imap</c> payloads.
    /// </summary>
    public const uint Director5ArchiveVersion = 0x000004C1;

    /// <summary>
    /// Director 6 archive version marker observed in <c>imap</c> payloads.
    /// </summary>
    public const uint Director6ArchiveVersion = 0x000004C7;

    /// <summary>
    /// Director 8.5 archive version marker observed in <c>imap</c> payloads.
    /// </summary>
    public const uint Director85ArchiveVersion = 0x00000708;

    /// <summary>
    /// Director 10 archive version marker observed in <c>imap</c> payloads.
    /// </summary>
    public const uint Director10ArchiveVersion = 0x00000742;

    /// <summary>
    /// Director 10.1 archive version marker observed in later <c>imap</c> payloads. Writers use this
    /// by default so modern archives report the most recent classic release.
    /// </summary>
    public const uint Director101ArchiveVersion = 0x00000744;
}

/// <summary>
/// Enumerates the Director releases supported by the legacy writers. The values are exposed through
/// <see cref="BlLegacyDirectorVersionExtensions"/> so callers can map high-level version selections to
/// the numeric archive markers stored inside <c>imap</c> chunks.
/// </summary>
public enum BlLegacyDirectorVersion
{
    /// <summary>
    /// Sentinel value used when the requested Director version is unspecified.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Director 4 (0x00000000) classic archives.
    /// </summary>
    Director4 = 4,

    /// <summary>
    /// Director 5 (0x000004C1) classic archives.
    /// </summary>
    Director5 = 5,

    /// <summary>
    /// Director 6 (0x000004C7) classic archives.
    /// </summary>
    Director6 = 6,

    /// <summary>
    /// Director 8.5 (0x00000708) classic archives.
    /// </summary>
    Director85 = 85,

    /// <summary>
    /// Director 10 (0x00000742) classic archives.
    /// </summary>
    Director10 = 10,

    /// <summary>
    /// Director 10.1 (0x00000744) classic archives. This represents the final pre-Projector release.
    /// </summary>
    Director101 = 101,

    /// <summary>
    /// Alias that resolves to the newest classic Director release supported by the writers.
    /// </summary>
    Latest = Director101
}

/// <summary>
/// Helper extensions that map <see cref="BlLegacyDirectorVersion"/> values to the numeric constants
/// consumed by <see cref="BlLegacyFormatConstants"/> and exposed through the archive metadata.
/// </summary>
public static class BlLegacyDirectorVersionExtensions
{
    /// <summary>
    /// Maps a logical Director version to the 32-bit archive marker emitted in <c>imap</c> payloads.
    /// </summary>
    /// <param name="version">The logical Director release requested by the caller.</param>
    /// <returns>The archive marker recorded inside the <c>imap</c> payload.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="version"/> is <see cref="BlLegacyDirectorVersion.Unknown"/>.</exception>
    public static uint ToArchiveVersionMarker(this BlLegacyDirectorVersion version) => version switch
    {
        BlLegacyDirectorVersion.Director4 => BlLegacyFormatConstants.Director4ArchiveVersion,
        BlLegacyDirectorVersion.Director5 => BlLegacyFormatConstants.Director5ArchiveVersion,
        BlLegacyDirectorVersion.Director6 => BlLegacyFormatConstants.Director6ArchiveVersion,
        BlLegacyDirectorVersion.Director85 => BlLegacyFormatConstants.Director85ArchiveVersion,
        BlLegacyDirectorVersion.Director10 => BlLegacyFormatConstants.Director10ArchiveVersion,
        BlLegacyDirectorVersion.Director101 => BlLegacyFormatConstants.Director101ArchiveVersion,
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Director version must be specified.")
    };

    /// <summary>
    /// Maps the logical Director version to the memory-map marker emitted alongside the archive
    /// metadata. Classic movies reuse a map version of <c>1</c> regardless of the targeted release.
    /// </summary>
    /// <param name="version">The logical Director release requested by the caller.</param>
    /// <returns>The map version value recorded in the <c>imap</c> payload.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="version"/> is <see cref="BlLegacyDirectorVersion.Unknown"/>.</exception>
    public static uint ToMapVersion(this BlLegacyDirectorVersion version)
    {
        if (version == BlLegacyDirectorVersion.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, "Director version must be specified.");
        }

        return BlLegacyFormatConstants.ClassicMapVersion;
    }

    /// <summary>
    /// Converts the logical Director version to the human-readable major version label exposed by
    /// <see cref="BlDataFormat"/>. Director 8.5 is reported as "8.5" to match the authoring tooling.
    /// </summary>
    /// <param name="version">The logical Director release requested by the caller.</param>
    /// <returns>The formatted label presented to diagnostics.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="version"/> is <see cref="BlLegacyDirectorVersion.Unknown"/>.</exception>
    public static string ToDirectorLabel(this BlLegacyDirectorVersion version) => version switch
    {
        BlLegacyDirectorVersion.Director4 => "Director 4",
        BlLegacyDirectorVersion.Director5 => "Director 5",
        BlLegacyDirectorVersion.Director6 => "Director 6",
        BlLegacyDirectorVersion.Director85 => "Director 8",
        BlLegacyDirectorVersion.Director10 => "Director 10",
        BlLegacyDirectorVersion.Director101 => "Director 10.1",
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Director version must be specified.")
    };

    /// <summary>
    /// Converts the logical Director version to the numeric major version used by
    /// <see cref="BlDataFormat.DirectorVersion"/>.
    /// </summary>
    /// <param name="version">The logical Director release requested by the caller.</param>
    /// <returns>The Director major version number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="version"/> is <see cref="BlLegacyDirectorVersion.Unknown"/>.</exception>
    public static int ToMajorVersionNumber(this BlLegacyDirectorVersion version) => version switch
    {
        BlLegacyDirectorVersion.Director4 => 4,
        BlLegacyDirectorVersion.Director5 => 5,
        BlLegacyDirectorVersion.Director6 => 6,
        BlLegacyDirectorVersion.Director85 => 8,
        BlLegacyDirectorVersion.Director10 => 10,
        BlLegacyDirectorVersion.Director101 => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Director version must be specified.")
    };

    /// <summary>
    /// Resolves the logical Director version from a raw archive marker stored inside an <c>imap</c>
    /// payload.
    /// </summary>
    /// <param name="archiveVersion">Numeric archive marker decoded from the container.</param>
    /// <returns>The corresponding <see cref="BlLegacyDirectorVersion"/> value, or <see cref="BlLegacyDirectorVersion.Unknown"/> when the marker is not recognised.</returns>
    public static BlLegacyDirectorVersion FromArchiveVersion(uint archiveVersion) => archiveVersion switch
    {
        BlLegacyFormatConstants.Director4ArchiveVersion => BlLegacyDirectorVersion.Director4,
        BlLegacyFormatConstants.Director5ArchiveVersion => BlLegacyDirectorVersion.Director5,
        BlLegacyFormatConstants.Director6ArchiveVersion => BlLegacyDirectorVersion.Director6,
        BlLegacyFormatConstants.Director85ArchiveVersion => BlLegacyDirectorVersion.Director85,
        BlLegacyFormatConstants.Director10ArchiveVersion => BlLegacyDirectorVersion.Director10,
        BlLegacyFormatConstants.Director101ArchiveVersion => BlLegacyDirectorVersion.Director101,
        _ => BlLegacyDirectorVersion.Unknown
    };
}
