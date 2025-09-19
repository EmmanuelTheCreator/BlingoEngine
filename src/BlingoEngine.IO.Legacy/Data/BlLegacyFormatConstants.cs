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
