using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Tools;

/// <summary>
/// Wraps a <see cref="Stream"/> with endian-aware helpers for producing Director byte sequences.
/// </summary>
public sealed class BlStreamWriter
{
    private readonly Stream _baseStream;
    private readonly byte[] _scratch = new byte[8];

    /// <summary>
    /// Initializes a new instance of the <see cref="BlStreamWriter"/> class.
    /// </summary>
    /// <param name="stream">Underlying stream that receives the Director bytes.</param>
    public BlStreamWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
        }

        _baseStream = stream;
    }

    /// <summary>
    /// Gets the underlying stream receiving the Director bytes.
    /// </summary>
    public Stream BaseStream => _baseStream;

    /// <summary>
    /// Gets or sets the byte ordering used when encoding multi-byte values.
    /// </summary>
    public BlEndianness Endianness { get; set; } = BlEndianness.BigEndian;

    /// <summary>
    /// Gets or sets the current byte offset within the stream.
    /// </summary>
    public long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Seek(value, SeekOrigin.Begin);
    }

    /// <summary>
    /// Gets the total stream length in bytes.
    /// </summary>
    public long Length => _baseStream.Length;

    /// <summary>
    /// Flushes the underlying stream.
    /// </summary>
    public void Flush() => _baseStream.Flush();

    /// <summary>
    /// Writes an unsigned byte to the current stream position.
    /// </summary>
    /// <param name="value">The byte value to write.</param>
    public void WriteByte(byte value) => _baseStream.WriteByte(value);

    /// <summary>
    /// Writes a signed byte to the current stream position.
    /// </summary>
    /// <param name="value">The signed byte value to write.</param>
    public void WriteSByte(sbyte value) => WriteByte(unchecked((byte)value));

    /// <summary>
    /// Writes the provided bytes directly to the underlying stream.
    /// </summary>
    /// <param name="source">Bytes to copy into the stream.</param>
    public void WriteBytes(ReadOnlySpan<byte> source) => _baseStream.Write(source);

    /// <summary>
    /// Writes the specified text using ASCII encoding without any endianness conversion.
    /// </summary>
    /// <param name="text">Text to encode using ASCII.</param>
    public void WriteAscii(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
        {
            return;
        }

        var bytes = Encoding.ASCII.GetBytes(text);
        _baseStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Writes a four-character tag using the writer's configured endianness.
    /// </summary>
    /// <param name="tag">Tag value to encode.</param>
    public void WriteTag(string tag) => WriteTag(tag, Endianness);

    /// <summary>
    /// Writes a four-character tag using the specified byte ordering.
    /// </summary>
    /// <param name="tag">Tag value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the tag.</param>
    public void WriteTag(string tag, BlEndianness endianness)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (tag.Length != 4)
        {
            throw new ArgumentException("Tags must contain exactly four characters.", nameof(tag));
        }

        Span<byte> buffer = stackalloc byte[4];
        Encoding.ASCII.GetBytes(tag, buffer);

        if (endianness == BlEndianness.LittleEndian)
        {
            buffer.Reverse();
        }

        _baseStream.Write(buffer);
    }

    /// <summary>
    /// Writes a four-character tag using the writer's configured endianness.
    /// </summary>
    /// <param name="tag">Tag to encode.</param>
    public void WriteTag(BlTag tag) => WriteTag(tag, Endianness);

    /// <summary>
    /// Writes a four-character tag using the specified byte ordering.
    /// </summary>
    /// <param name="tag">Tag to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the tag.</param>
    public void WriteTag(BlTag tag, BlEndianness endianness) => WriteTag(tag.Value, endianness);

    /// <summary>
    /// Writes an unsigned 16-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteUInt16(ushort value) => WriteUInt16(value, Endianness);

    /// <summary>
    /// Writes an unsigned 16-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteUInt16(ushort value, BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 2);
        if (endianness == BlEndianness.LittleEndian)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt16BigEndian(span, value);
        }

        _baseStream.Write(span);
    }

    /// <summary>
    /// Writes a signed 16-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteInt16(short value) => WriteUInt16(unchecked((ushort)value));

    /// <summary>
    /// Writes a signed 16-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteInt16(short value, BlEndianness endianness) => WriteUInt16(unchecked((ushort)value), endianness);

    /// <summary>
    /// Writes an unsigned 32-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteUInt32(uint value) => WriteUInt32(value, Endianness);

    /// <summary>
    /// Writes an unsigned 32-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteUInt32(uint value, BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 4);
        if (endianness == BlEndianness.LittleEndian)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt32BigEndian(span, value);
        }

        _baseStream.Write(span);
    }

    /// <summary>
    /// Writes a signed 32-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteInt32(int value) => WriteUInt32(unchecked((uint)value));

    /// <summary>
    /// Writes a signed 32-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteInt32(int value, BlEndianness endianness) => WriteUInt32(unchecked((uint)value), endianness);

    /// <summary>
    /// Writes an unsigned 64-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteUInt64(ulong value) => WriteUInt64(value, Endianness);

    /// <summary>
    /// Writes an unsigned 64-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteUInt64(ulong value, BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 8);
        if (endianness == BlEndianness.LittleEndian)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt64BigEndian(span, value);
        }

        _baseStream.Write(span);
    }

    /// <summary>
    /// Writes a signed 64-bit integer respecting the writer's configured endianness.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    public void WriteInt64(long value) => WriteUInt64(unchecked((ulong)value));

    /// <summary>
    /// Writes a signed 64-bit integer using the specified byte ordering.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <param name="endianness">Byte ordering used when writing the value.</param>
    public void WriteInt64(long value, BlEndianness endianness) => WriteUInt64(unchecked((ulong)value), endianness);
}
