namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents the four-field payload found after the <c>imap</c> tag. According to byte dumps, the block stores the map length,
/// map version, offset to the <c>mmap</c> table, and the archive version (a 32-bit Director build marker).
/// </summary>
/// <param name="Length">Length of the <c>imap</c> payload in bytes.</param>
/// <param name="MapVersion">Format version recorded by Director (commonly 0 or 1).</param>
/// <param name="MapOffset">Absolute offset pointing to the <c>mmap</c> block.</param>
/// <param name="ArchiveVersion">Director archive marker that maps to releases such as 4, 5, 6, 8.5, and 10.</param>
public readonly record struct BlImapBlock(uint Length, uint MapVersion, uint MapOffset, uint ArchiveVersion);
