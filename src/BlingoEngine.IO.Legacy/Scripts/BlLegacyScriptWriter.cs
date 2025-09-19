using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scripts;

/// <summary>
/// Describes how a legacy <c>CASt</c> info payload stores the script text and name.
/// </summary>
internal enum BlLegacyScriptInfoLayout
{
    /// <summary>
    /// Director 4 and later expose the script bytes after the pointer table slot at offset <c>0x0068</c>.
    /// </summary>
    PointerTable = 0,

    /// <summary>
    /// Older exports omit the pointer table and place the script text immediately after the stored length field.
    /// </summary>
    LegacyTextAfterLength = 1
}

/// <summary>
/// Emits synthetic cast-member payloads for tests that validate the legacy script reader. The writer mirrors the
/// <c>CASt</c> info layout documented in <c>docs/LegacyScriptMembers.md</c> so future write-back code can reuse the same
/// helpers when rebuilding Director archives.
/// </summary>
internal sealed class BlLegacyScriptWriter
{
    private const int ScriptMemberType = 11;
    private const int ScriptNumberOffset = 16;
    private const int ScriptResourceIdOffset = 8;
    private const int ScriptTextLengthOffset = 0x1D;
    private const int PointerSlotOffset = 0x68;
    private const int PointerTextStart = 0x6A;
    private const int LegacyTextStart = ScriptTextLengthOffset + sizeof(uint);

    private static readonly BlTag CastTag = BlTag.Cast;
    private static readonly BlTag ScriptTag = BlTag.Register("Lscr");

    private readonly BlStreamWriter _writer;

    public BlLegacyScriptWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    public BlLegacyResourceEntry WriteCastScript(
        int resourceId,
        int scriptNumber,
        int scriptResourceId,
        BlLegacyScriptFormatKind format,
        string? scriptText,
        string? scriptName,
        BlLegacyScriptInfoLayout layout = BlLegacyScriptInfoLayout.PointerTable)
    {
        if (resourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(resourceId), "Resource identifiers must be positive.");
        if (scriptNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(scriptNumber), "Script numbers must be positive.");
        if (scriptResourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(scriptResourceId), "Script resource ids must be positive.");

        var textBytes = scriptText is null ? Array.Empty<byte>() : Encoding.Latin1.GetBytes(scriptText);
        var nameBytes = scriptName is null ? Array.Empty<byte>() : Encoding.Latin1.GetBytes(scriptName);

        if (nameBytes.Length > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(scriptName), "Script names must fit in a single length byte.");

        var infoPayload = BuildInfoPayload(scriptNumber, scriptResourceId, textBytes, nameBytes, layout);
        var specificPayload = BuildSpecificPayload(format);

        var payloadLength = checked(12 + infoPayload.Length + specificPayload.Length);
        var payload = new byte[payloadLength];

        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(0, 4), ScriptMemberType);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)infoPayload.Length);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), (uint)specificPayload.Length);

        infoPayload.CopyTo(payload.AsSpan(12));
        specificPayload.CopyTo(payload.AsSpan(12 + infoPayload.Length));

        return WriteChunk(resourceId, CastTag, payload);
    }

    public BlLegacyResourceEntry WriteScriptResource(int resourceId, ReadOnlySpan<byte> payload)
        => WriteChunk(resourceId, ScriptTag, payload);

    private BlLegacyResourceEntry WriteChunk(int resourceId, BlTag tag, ReadOnlySpan<byte> payload)
    {
        if (resourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(resourceId), "Resource identifiers must be positive.");

        var offset = _writer.Position;
        if (offset < 0 || offset > uint.MaxValue)
            throw new InvalidOperationException("Chunk offset exceeds the legacy map range.");

        var declaredSize = checked((uint)payload.Length);

        _writer.WriteTag(tag);
        _writer.WriteUInt32(declaredSize);
        _writer.WriteBytes(payload);

        return new BlLegacyResourceEntry(resourceId, tag, declaredSize, (uint)offset, 0, 0, 0);
    }

    private static byte[] BuildInfoPayload(
        int scriptNumber,
        int scriptResourceId,
        ReadOnlySpan<byte> textBytes,
        ReadOnlySpan<byte> nameBytes,
        BlLegacyScriptInfoLayout layout)
    {
        var textLength = textBytes.Length;
        var nameLength = nameBytes.Length;
        var textStart = layout == BlLegacyScriptInfoLayout.PointerTable ? PointerTextStart : LegacyTextStart;

        var minimumLength = Math.Max(ScriptTextLengthOffset + sizeof(uint), textStart);
        var tailLength = 1 + nameLength;
        var requiredLength = Math.Max(minimumLength, textStart + textLength + tailLength);

        if (layout == BlLegacyScriptInfoLayout.PointerTable)
            requiredLength = Math.Max(requiredLength, PointerSlotOffset + sizeof(ushort));

        var info = new byte[requiredLength];

        BinaryPrimitives.WriteUInt32BigEndian(info.AsSpan(ScriptResourceIdOffset, sizeof(uint)), unchecked((uint)scriptResourceId));
        BinaryPrimitives.WriteInt32BigEndian(info.AsSpan(ScriptNumberOffset, sizeof(int)), scriptNumber);
        BinaryPrimitives.WriteUInt32LittleEndian(info.AsSpan(ScriptTextLengthOffset, sizeof(uint)), (uint)textLength);

        if (layout == BlLegacyScriptInfoLayout.PointerTable)
            BinaryPrimitives.WriteUInt16BigEndian(info.AsSpan(PointerSlotOffset, sizeof(ushort)), (ushort)textStart);

        if (textLength > 0)
            textBytes.CopyTo(info.AsSpan(textStart, textLength));

        var nameIndex = textStart + textLength;
        if (nameIndex >= info.Length)
        {
            Array.Resize(ref info, nameIndex + tailLength);
        }

        info[nameIndex] = (byte)nameLength;
        if (nameLength > 0)
            nameBytes.CopyTo(info.AsSpan(nameIndex + 1, nameLength));

        return info;
    }

    private static byte[] BuildSpecificPayload(BlLegacyScriptFormatKind format)
    {
        var selector = BlLegacyScriptFormat.MapSelector((byte)format);
        if (selector == BlLegacyScriptFormatKind.Unknown)
        {
            selector = format;
        }

        var code = (byte)selector;
        return new[] { (byte)0x00, code };
    }
}
