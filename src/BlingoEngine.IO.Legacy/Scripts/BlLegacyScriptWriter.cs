using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast;
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
/// Provides helpers for synthesising behaviour script libraries. The builder emits the KEY*, CAS*, CASt, and Lscr resources
/// required to persist a single behaviour member using the legacy file writers.
/// </summary>
public static class BlLegacyBehaviorLibraryBuilder
{
    private const uint KeyResourceIndex = 0;
    private const uint CastTableResourceIndex = 1;
    private const uint CastMemberResourceIndex = 2;
    private const uint ScriptResourceIndex = 3;
    private const uint ScriptContextResourceIndex = 4;

    private const uint KeyResourceId = KeyResourceIndex + 1;
    private const uint CastTableResourceId = CastTableResourceIndex + 1;
    private const uint CastMemberResourceId = CastMemberResourceIndex + 1;
    private const uint ScriptResourceId = ScriptResourceIndex + 1;
    private const uint ScriptContextResourceId = ScriptContextResourceIndex + 1;

    private static readonly BlTag ScriptTag = BlTag.Get("Lscr");
    private static readonly BlTag ScriptContextTag = BlTag.Get("Lctx");

    /// <summary>
    /// Builds a <see cref="DirFilesContainerDTO"/> containing a single behaviour cast member backed by an Lscr resource.
    /// </summary>
    /// <param name="memberName">Display name stored in the behaviour metadata.</param>
    /// <param name="scriptText">Behaviour source code written to the CASt info payload.</param>
    /// <returns>A container populated with the resources necessary to emit the behaviour library.</returns>
    public static DirFilesContainerDTO BuildSingleMemberBehaviorLibrary(string? memberName, string? scriptText)
    {
        var container = new DirFilesContainerDTO();

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.KeyStar.Value}_{KeyResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildKeyTable(new[]
            {
                new BlLegacyCastLibraryBuilderHelpers.KeyTableEntry(ScriptResourceIndex, CastMemberResourceIndex, ScriptTag)
            })
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.CasStar.Value}_{CastTableResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildCastTable(CastMemberResourceIndex)
        });

        using var stream = new MemoryStream();
        var writer = new BlLegacyScriptWriter(stream);

        var scriptId = (int)ScriptResourceIndex;
        var castId = (int)CastMemberResourceIndex;
        const int scriptNumber = 1;

        var normalizedText = NormalizeScriptText(scriptText);

        var castEntry = writer.WriteCastScript(
            castId,
            scriptNumber: scriptNumber,
            scriptResourceId: scriptId,
            BlLegacyScriptFormatKind.Behavior,
            normalizedText,
            memberName);

        var scriptEntry = writer.WriteScriptResource(scriptId, new byte[] { 0x00 });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"CASt_{CastMemberResourceId:D4}.bin",
            Bytes = ExtractPayload(stream, castEntry)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{ScriptTag.Value}_{ScriptResourceId:D4}.bin",
            Bytes = ExtractPayload(stream, scriptEntry)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{ScriptContextTag.Value}_{ScriptContextResourceId:D4}.bin",
            Bytes = BuildScriptContextPayload(scriptId)
        });

        return container;
    }

    private static string? NormalizeScriptText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text.Replace("\r\n", "\n").Replace('\r', '\n').Replace('\n', '\r');
    }

    private static byte[] ExtractPayload(MemoryStream stream, BlLegacyResourceEntry entry)
    {
        var length = (int)entry.DeclaredSize;
        if (length == 0)
        {
            return Array.Empty<byte>();
        }

        var offset = (int)entry.MapOffset;
        var buffer = stream.ToArray();
        var payloadStart = offset + 8;
        if (payloadStart < 0 || buffer.Length < payloadStart + length)
        {
            throw new InvalidOperationException("Script payload exceeds the available buffer length.");
        }

        var payload = new byte[length];
        Array.Copy(buffer, payloadStart, payload, 0, length);
        return payload;
    }

    private static byte[] BuildScriptContextPayload(int scriptResourceId)
    {
        const int headerLength = 0x2A;
        const int entryLength = 12;

        var payload = new byte[headerLength + entryLength];
        var span = payload.AsSpan();

        BinaryPrimitives.WriteUInt32BigEndian(span.Slice(8, 4), 1u);
        BinaryPrimitives.WriteUInt32BigEndian(span.Slice(12, 4), 1u);
        BinaryPrimitives.WriteUInt16BigEndian(span.Slice(16, 2), (ushort)headerLength);

        BinaryPrimitives.WriteInt32BigEndian(span.Slice(headerLength + 4, 4), scriptResourceId);

        return payload;
    }
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
    private static readonly BlTag ScriptTag = BlTag.Get("Lscr");

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
