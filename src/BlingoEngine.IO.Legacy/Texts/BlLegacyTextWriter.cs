using System;
using System.IO;
using System.Text;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Texts;

/// <summary>
/// Encodes synthetic text resources for test scenarios. The writer mirrors the classic eight-byte
/// chunk prefix described in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> — a
/// four-character tag followed by a big-endian payload length — so
/// <see cref="BlClassicPayloadLoader"/> can resolve the bytes across every Director generation.
/// </summary>
internal sealed class BlLegacyTextWriter
{
    private static readonly BlTag StxtTag = BlTag.Get("STXT");
    private static readonly BlTag XmedTag = BlTag.Get("XMED");

    private readonly BlStreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyTextWriter"/> class.
    /// </summary>
    /// <param name="stream">Destination stream that receives the text chunks.</param>
    public BlLegacyTextWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    /// <summary>
    /// Writes a classic <c>STXT</c> chunk that Director 2–10 projectors associate with static text
    /// and editable fields. The payload typically contains MacRoman characters or Pascal strings as
    /// captured in the byte tables documented for those versions.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw <c>STXT</c> bytes copied from the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> pointing at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteStxt(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, StxtTag, payload);

    /// <summary>
    /// Writes a styled <c>XMED</c> chunk that later Director versions emit alongside cast headers.
    /// The payload includes style runs and colour tables laid out according to the offsets captured
    /// in <c>XMED_Offsets.md</c>.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw <c>XMED</c> bytes preserved for downstream decoding.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> pointing at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteXmed(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, XmedTag, payload);

    private BlLegacyResourceEntry WriteResource(int resourceId, BlTag tag, ReadOnlySpan<byte> payload)
    {
        if (resourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(resourceId), "Resource identifiers must be positive.");

        var payloadLength = payload.Length;
        var declaredSize = checked((uint)payloadLength);

        var offset = _writer.Position;
        if (offset < 0 || offset > uint.MaxValue)
            throw new InvalidOperationException("Chunk offset exceeds the range supported by legacy maps.");

        _writer.WriteTag(tag);
        _writer.WriteUInt32(declaredSize);
        _writer.WriteBytes(payload);

        return new BlLegacyResourceEntry(resourceId, tag, declaredSize, (uint)offset, 0, 0, 0);
    }
}

/// <summary>
/// Provides helpers for synthesising minimal text cast libraries. The builder mirrors the KEY*/CAS*/CASt
/// layout used by modern Director authoring movies so the resulting container can be persisted through the
/// legacy file writers.
/// </summary>
public static class BlLegacyTextLibraryBuilder
{
    private const uint KeyResourceIndex = 0;
    private const uint CastTableResourceIndex = 1;
    private const uint CastMemberResourceIndex = 2;
    private const uint TextResourceIndex = 3;

    private const uint KeyResourceId = KeyResourceIndex + 1;
    private const uint CastTableResourceId = CastTableResourceIndex + 1;
    private const uint CastMemberResourceId = CastMemberResourceIndex + 1;
    private const uint TextResourceId = TextResourceIndex + 1;

    private static readonly BlTag StxtTag = BlTag.Get("STXT");

    private static readonly Encoding TextEncoding = Encoding.Latin1;

    /// <summary>
    /// Builds a <see cref="DirFilesContainerDTO"/> containing a single text cast member backed by an STXT resource.
    /// </summary>
    /// <param name="memberName">Display name stored in the cast metadata.</param>
    /// <param name="text">Text payload written to the STXT chunk.</param>
    /// <returns>A container populated with the resources required to emit a cast library.</returns>
    public static DirFilesContainerDTO BuildSingleMemberTextLibrary(string? memberName, string? text)
    {
        var container = new DirFilesContainerDTO();

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.KeyStar.Value}_{KeyResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildKeyTable(new[]
            {
                new BlLegacyCastLibraryBuilderHelpers.KeyTableEntry(TextResourceIndex, CastMemberResourceIndex, StxtTag)
            })
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.CasStar.Value}_{CastTableResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildCastTable(CastMemberResourceIndex)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"CASt_{CastMemberResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildModernCastMetadata(
                BlLegacyCastMemberType.Text,
                memberName,
                dataLength: 0)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{StxtTag.Value}_{TextResourceId:D4}.bin",
            Bytes = BuildStxtPayload(text)
        });

        return container;
    }

    private static byte[] BuildStxtPayload(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<byte>();
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace('\n', '\r');
        return TextEncoding.GetBytes(normalized);
    }
}
