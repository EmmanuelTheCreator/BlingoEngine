using System;
using System.IO;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Entry point that loads Director archives and projector files, traversing the 12-byte movie headers and map bytes to return
/// DTOs built from the embedded resource payloads.
/// </summary>
public sealed class BlLegacyReader
{
    /// <summary>
    /// Reads the Director container located at <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Absolute or relative path to the archive or projector file.</param>
    /// <returns>A DTO container describing the decoded resource bytes.</returns>
    public DirFilesContainerDTO Read(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        using var stream = File.OpenRead(path);
        using var context = new ReaderContext(stream, Path.GetFileName(path), leaveOpen: false);
        var file = CreateFileReader(path, context);
        return file.Read();
    }

    /// <summary>
    /// Reads a Director container from an existing stream.
    /// </summary>
    /// <param name="stream">The source stream positioned at the beginning of the movie bytes.</param>
    /// <param name="fileName">Logical name used to infer the file type.</param>
    /// <param name="leaveOpen">Whether the supplied stream should remain open after parsing.</param>
    /// <returns>A DTO container describing the decoded resource bytes.</returns>
    public DirFilesContainerDTO Read(Stream stream, string fileName, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);
        using var context = new ReaderContext(stream, fileName, leaveOpen);
        var file = CreateFileReader(fileName, context);
        return file.Read();
    }

    /// <summary>
    /// Creates the appropriate reader for the supplied file extension.
    /// </summary>
    private static BlLegacyFileResourceBase CreateFileReader(string fileName, ReaderContext context)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".cst" => new BlCstFile(context),
            ".dcr" or ".dxr" => new BlDcrFile(context),
            ".dir" => new BlDirFile(context),
            _ => new BlDirFile(context)
        };
    }
}
