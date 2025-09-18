using System;
using System.IO;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Infrastructure;

/// <summary>
/// Reads the 12-byte <c>RIFX/XFIR</c> header and associated codec tag to populate a <see cref="BlDataBlock"/>.
/// </summary>
public sealed class BlFormatReader
{
    /// <summary>
    /// Reads the top-level header and returns a <see cref="BlDataBlock"/> containing payload size and format information.
    /// </summary>
    /// <param name="context">Reader context pointing at the 12-byte header bytes.</param>
    /// <returns>The decoded data block describing payload size and codec metadata.</returns>
    public BlDataBlock Read(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stream = context.BaseStream;
        var reader = context.Reader;
        var start = context.RifxOffset;

        stream.Seek(start, SeekOrigin.Begin);
        reader.Position = start;

        var magic = reader.ReadTag();
        bool isBigEndian = magic == BlTag.RIFX;
        if (!isBigEndian && magic != BlTag.XFIR)
        {
            throw new InvalidDataException($"Unsupported container magic '{magic}'.");
        }

        reader.Endianness = isBigEndian ? BlEndianness.BigEndian : BlEndianness.LittleEndian;

        var payloadSize = reader.ReadUInt32();
        var codecTag = reader.ReadTag();

        var block = new BlDataBlock
        {
            DeclaredSize = payloadSize,
            PayloadStart = reader.Position,
            PayloadEnd = reader.Position + payloadSize
        };

        block.Format.Codec = MapCodec(codecTag);
        block.Format.IsBigEndian = isBigEndian;

        return block;
    }

    private static BlRifxCodec MapCodec(BlTag tag)
    {
        if (tag == BlTag.MV93)
        {
            return BlRifxCodec.MV93;
        }

        if (tag == BlTag.MC95)
        {
            return BlRifxCodec.MC95;
        }

        if (tag == BlTag.APPL)
        {
            return BlRifxCodec.APPL;
        }

        if (tag == BlTag.FGDM)
        {
            return BlRifxCodec.FGDM;
        }

        if (tag == BlTag.FGDC)
        {
            return BlRifxCodec.FGDC;
        }

        return BlRifxCodec.Unknown;
    }
}

/// <summary>
/// Convenience helpers that extend <see cref="ReaderContext"/> with header-reading capabilities.
/// </summary>
internal static class BlFormatReaderExtensions
{
    /// <summary>
    /// Reads the next data block header from the underlying stream and registers it with the context.
    /// </summary>
    /// <param name="context">Reader context positioned at the start of a Director movie header.</param>
    /// <returns>The decoded block describing payload boundaries and codec metadata.</returns>
    public static BlDataBlock ReadBlockHeader(this ReaderContext context)
    {
        var reader = new BlFormatReader();
        var block = reader.Read(context);
        context.RegisterDataBlock(block);
        return block;
    }
}
