using System;
using System.Buffers.Binary;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Enumerates container formats detected in legacy Director sound resources. The reader only
/// inspects the leading bytes of the payload, so the classification is lightweight and tolerant
/// of uncommon codecs that may appear in the wild.
/// </summary>
public enum BlLegacySoundFormatKind
{
    /// <summary>
    /// The payload could not be classified using the known file headers.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// MPEG Layer III stream identified by an ID3 tag or an MP3 frame sync word.
    /// </summary>
    Mp3,

    /// <summary>
    /// Waveform audio stored in RIFF/FFIR/RIFX containers.
    /// </summary>
    Wave,

    /// <summary>
    /// AIFF container detected via the FORM/AIFF header.
    /// </summary>
    Aiff,

    /// <summary>
    /// AIFF-C container detected via the FORM/AIFC header.
    /// </summary>
    Aifc,

    /// <summary>
    /// MIDI sequence starting with the standard MThd signature.
    /// </summary>
    Midi,

    /// <summary>
    /// Apple Core Audio Format (CAF) stream.
    /// </summary>
    Caf,

    /// <summary>
    /// Ogg container (typically Vorbis) detected via the OggS marker.
    /// </summary>
    Ogg,

    /// <summary>
    /// Free Lossless Audio Codec stream detected via the fLaC marker.
    /// </summary>
    Flac,

    /// <summary>
    /// Sun/NeXT AU container identified by the .snd signature.
    /// </summary>
    Au,

    /// <summary>
    /// MPEG-4 container detected by the ftyp box near the start of the payload.
    /// </summary>
    Mp4
}

/// <summary>
/// Provides helpers for classifying legacy sound payloads based on their file signatures.
/// </summary>
internal static class BlLegacySoundFormat
{
    public static BlLegacySoundFormatKind Detect(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 3 && data[0] == (byte)'I' && data[1] == (byte)'D' && data[2] == (byte)'3')
            return BlLegacySoundFormatKind.Mp3;

        if (data.Length >= 4)
        {
            var header = BinaryPrimitives.ReadUInt32BigEndian(data);
            switch (header)
            {
                case 0x52494646: // RIFF
                case 0x46464952: // FFIR
                case 0x52494658: // RIFX
                    return BlLegacySoundFormatKind.Wave;
                case 0x464F524D: // FORM
                    if (data.Length >= 12)
                    {
                        var subType = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4));
                        if (subType == 0x41494646) // AIFF
                            return BlLegacySoundFormatKind.Aiff;
                        if (subType == 0x41494643) // AIFC
                            return BlLegacySoundFormatKind.Aifc;
                    }
                    return BlLegacySoundFormatKind.Aiff;
                case 0x4F676753: // OggS
                    return BlLegacySoundFormatKind.Ogg;
                case 0x664C6143: // fLaC
                    return BlLegacySoundFormatKind.Flac;
                case 0x2E736E64: // .snd
                    return BlLegacySoundFormatKind.Au;
                case 0x4D546864: // MThd
                    return BlLegacySoundFormatKind.Midi;
                case 0x63616666: // caff
                case 0x43414646: // CAFF
                    return BlLegacySoundFormatKind.Caf;
            }
        }

        if (data.Length >= 8)
        {
            var box = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4));
            if (box == 0x66747970) // ftyp
                return BlLegacySoundFormatKind.Mp4;
        }

        if (data.Length >= 2 && data[0] == 0xFF && (data[1] & 0xE0) == 0xE0)
            return BlLegacySoundFormatKind.Mp3;

        return BlLegacySoundFormatKind.Unknown;
    }
}
