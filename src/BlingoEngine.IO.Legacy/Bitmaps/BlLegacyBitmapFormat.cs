using System;
using System.Buffers.Binary;

using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Enumerates container formats detected in legacy Director bitmap resources. The reader relies on
/// the resource tag and well-known file signatures to classify the payload so higher layers can
/// decide how to decode the pixel data.
/// </summary>
internal enum BlLegacyBitmapFormatKind
{
    /// <summary>
    /// The payload could not be classified using the known resource tags or signatures.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Macintosh BITD stream storing run-length encoded pixels.
    /// </summary>
    Bitd,

    /// <summary>
    /// Windows device-independent bitmap containing a BITMAPINFOHEADER and pixel data.
    /// </summary>
    Dib,

    /// <summary>
    /// Macintosh PICT drawing stored as a QuickDraw picture.
    /// </summary>
    Pict,

    /// <summary>
    /// Raw alpha mask bytes extracted from an ALFA resource.
    /// </summary>
    AlphaMask,

    /// <summary>
    /// Thumbnail bitmap stored in a Thum resource.
    /// </summary>
    Thumbnail,

    /// <summary>
    /// Portable Network Graphics stream detected via the PNG signature.
    /// </summary>
    Png,

    /// <summary>
    /// Joint Photographic Experts Group stream identified by the SOI marker.
    /// </summary>
    Jpeg,

    /// <summary>
    /// Graphics Interchange Format stream detected via the GIF header.
    /// </summary>
    Gif,

    /// <summary>
    /// Windows bitmap detected via the BM signature.
    /// </summary>
    Bmp,

    /// <summary>
    /// Tagged Image File Format stream detected via the TIFF headers.
    /// </summary>
    Tiff
}

/// <summary>
/// Provides helpers for classifying legacy bitmap payloads based on resource tags and magic numbers.
/// </summary>
internal static class BlLegacyBitmapFormat
{
    private static readonly BlTag BitdTag = BlTag.Register("BITD");
    private static readonly BlTag DibTag = BlTag.Register("DIB ");
    private static readonly BlTag PictTag = BlTag.Register("PICT");
    private static readonly BlTag AlphaTag = BlTag.Register("ALFA");
    private static readonly BlTag ThumbTag = BlTag.Register("Thum");

    public static BlLegacyBitmapFormatKind Detect(BlTag tag, ReadOnlySpan<byte> data)
    {
        if (tag == BitdTag)
            return BlLegacyBitmapFormatKind.Bitd;

        if (tag == DibTag)
            return BlLegacyBitmapFormatKind.Dib;

        if (tag == PictTag)
            return BlLegacyBitmapFormatKind.Pict;

        if (tag == AlphaTag)
            return BlLegacyBitmapFormatKind.AlphaMask;

        if (tag == ThumbTag)
            return BlLegacyBitmapFormatKind.Thumbnail;

        if (IsPng(data))
            return BlLegacyBitmapFormatKind.Png;

        if (IsJpeg(data))
            return BlLegacyBitmapFormatKind.Jpeg;

        if (IsGif(data))
            return BlLegacyBitmapFormatKind.Gif;

        if (IsBmp(data))
            return BlLegacyBitmapFormatKind.Bmp;

        if (IsDib(data))
            return BlLegacyBitmapFormatKind.Dib;

        if (IsTiff(data))
            return BlLegacyBitmapFormatKind.Tiff;

        return BlLegacyBitmapFormatKind.Unknown;
    }

    private static bool IsPng(ReadOnlySpan<byte> data)
    {
        ReadOnlySpan<byte> png = stackalloc byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        return data.Length >= png.Length && data[..png.Length].SequenceEqual(png);
    }

    private static bool IsJpeg(ReadOnlySpan<byte> data)
    {
        return data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF;
    }

    private static bool IsGif(ReadOnlySpan<byte> data)
    {
        if (data.Length < 6)
            return false;

        return data[0] == (byte)'G'
            && data[1] == (byte)'I'
            && data[2] == (byte)'F'
            && data[3] == (byte)'8'
            && (data[4] == (byte)'7' || data[4] == (byte)'9')
            && data[5] == (byte)'a';
    }

    private static bool IsBmp(ReadOnlySpan<byte> data)
    {
        return data.Length >= 2 && data[0] == (byte)'B' && data[1] == (byte)'M';
    }

    private static bool IsDib(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return false;

        var header = BinaryPrimitives.ReadUInt32LittleEndian(data);
        return header == 0x0000000C
            || header == 0x00000028
            || header == 0x00000040
            || header == 0x0000006C
            || header == 0x0000007C;
    }

    private static bool IsTiff(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return false;

        var bigEndian = BinaryPrimitives.ReadUInt16BigEndian(data);
        var littleEndian = BinaryPrimitives.ReadUInt16LittleEndian(data);

        if (bigEndian == 0x4D4D && BinaryPrimitives.ReadUInt16BigEndian(data[2..]) == 0x002A)
            return true;

        if (littleEndian == 0x4949 && BinaryPrimitives.ReadUInt16LittleEndian(data[2..]) == 0x002A)
            return true;

        return false;
    }
}
