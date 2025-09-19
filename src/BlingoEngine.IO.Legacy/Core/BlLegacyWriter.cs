using System.IO;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Files;

namespace BlingoEngine.IO.Legacy.Core;

/// <summary>
/// Entry point for emitting Director-compatible archives from <see cref="DirFilesContainerDTO"/> instances.
/// </summary>
public sealed class BlLegacyWriter
{
    /// <summary>
    /// Writes the supplied container to <paramref name="path"/>, creating or overwriting the target file.
    /// </summary>
    /// <param name="path">Destination path for the serialized movie.</param>
    /// <param name="container">DTO container describing the resources to embed.</param>
    public void Write(string path, DirFilesContainerDTO container)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var stream = File.Create(path);
        Write(stream, path, container, leaveOpen: true);
    }

    /// <summary>
    /// Writes the supplied container to an existing stream.
    /// </summary>
    /// <param name="stream">Destination stream that receives the movie bytes.</param>
    /// <param name="fileName">Logical file name used to select the container writer.</param>
    /// <param name="container">DTO container describing the resources to embed.</param>
    /// <param name="leaveOpen">Whether the provided stream should remain open after the method returns.</param>
    public void Write(Stream stream, string fileName, DirFilesContainerDTO container, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(container);

        var writer = CreateFileWriter(fileName);
        try
        {
            writer.Write(stream, container);
        }
        finally
        {
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }
    }

    private static BlLegacyFileWriterBase CreateFileWriter(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".cst" => new BlCstFileWriter(),
            ".dct" => new BlDctFileWriter(),
            ".dir" => new BlDirFileWriter(),
            _ => new BlDirFileWriter()
        };
    }
}
