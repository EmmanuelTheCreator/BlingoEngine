using System;
using System.IO;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy;

/// <summary>
/// Provides shared state while traversing the byte-level Director container. The context exposes the base stream, the
/// endian-aware <see cref="BlStreamReader"/>, and caches for resources and compression descriptors decoded from control blocks.
/// </summary>
public sealed class ReaderContext : IDisposable
{
    private readonly bool _leaveOpen;
    private bool _disposed;

    public Stream BaseStream { get; }
    public BlStreamReader Reader { get; }
    public string FileName { get; }
    public bool LeaveOpen => _leaveOpen;
    public BlDataBlock? DataBlock { get; private set; }
    public long RifxOffset { get; private set; }
    public BlResourceContainer Resources { get; } = new();
    public BlCompressionContainer Compressions { get; } = new();

    /// <summary>
    /// Initializes a new <see cref="ReaderContext"/> over the provided stream. The reader consumes the 12-byte RIFX header,
    /// the memory-map control bytes, and any Afterburner metadata without taking ownership of the stream when requested.
    /// </summary>
    /// <param name="stream">Source stream containing the Director bytes.</param>
    /// <param name="fileName">Logical file name associated with the stream.</param>
    /// <param name="leaveOpen">Whether the stream should remain open after the context is disposed.</param>
    public ReaderContext(Stream stream, string fileName, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);

        _leaveOpen = leaveOpen;
        BaseStream = stream;
        Reader = new BlStreamReader(stream);
        FileName = fileName;
    }

    /// <summary>
    /// Clears the in-memory resource and compression catalogs so that a fresh archive can be parsed.
    /// </summary>
    public void ResetRegistries()
    {
        Resources.Reset();
        Compressions.Reset();
    }

    /// <summary>
    /// Records the stream offset where the <c>RIFX/XFIR</c> header begins.
    /// </summary>
    /// <param name="offset">Absolute position of the header inside the stream.</param>
    public void RegisterRifxOffset(long offset)
    {
        RifxOffset = offset;
    }

    /// <summary>
    /// Stores the data block that was read from the movie header.
    /// </summary>
    /// <param name="block">Header metadata describing payload boundaries.</param>
    public void RegisterDataBlock(BlDataBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        DataBlock = block;
    }

    /// <summary>
    /// Registers a resource entry decoded from the current map.
    /// </summary>
    /// <param name="entry">Resource metadata parsed from the archive.</param>
    public void AddResource(BlLegacyResourceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Resources.Add(entry);
    }

    /// <summary>
    /// Stores the inline payload bytes for a resource resolved from the Afterburner segment table.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the inline resource.</param>
    /// <param name="data">Payload bytes stored in the inline segment buffer.</param>
    public void SetResourceInlineSegment(int resourceId, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Resources.SetInlineSegment(resourceId, data);
    }

    /// <summary>
    /// Records a relationship exposed by the <c>KEY*</c> table.
    /// </summary>
    /// <param name="link">Descriptor that ties the child resource to its parent.</param>
    public void AddResourceRelationship(BlResourceKeyLink link)
    {
        Resources.AddRelationship(link);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (!_leaveOpen)
        {
            BaseStream.Dispose();
        }
    }

    /// <summary>
    /// Creates a new <see cref="BlStreamReader"/> over an in-memory buffer. This helper is used when Director encodes metadata
    /// as compressed byte arrays, such as the <c>ABMP</c> payload and inline segment tables.
    /// </summary>
    /// <param name="data">Byte array containing the stream to read.</param>
    /// <param name="endianness">Optional override for the reader endianness.</param>
    /// <returns>An endian-aware reader positioned at the start of the buffer.</returns>
    public BlStreamReader CreateMemoryReader(byte[] data, BlEndianness? endianness = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        var memory = new MemoryStream(data, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = endianness ?? Reader.Endianness
        };

        return reader;
    }
}
