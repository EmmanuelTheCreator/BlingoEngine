using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Files;

/// <summary>
/// Base class for legacy Director file writers. The derived types decide the container signature and
/// codec tag while this class emits the shared <c>imap</c>/<c>mmap</c> layout and resource chunks from
/// <see cref="DirFilesContainerDTO"/> instances.
/// </summary>
public abstract class BlLegacyFileWriterBase
{
    private const uint HeaderLength = 12;

    private readonly BlTag _codecTag;
    private readonly BlEndianness _endianness;
    private readonly uint _mapVersion;
    private readonly uint _archiveVersion;
    private readonly string _signature;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyFileWriterBase"/> class.
    /// </summary>
    /// <param name="codecTag">The four-character code written after the <c>RIFX/XFIR</c> header.</param>
    /// <param name="isBigEndian">Determines whether the container uses the big-endian <c>RIFX</c> tag.</param>
    /// <param name="directorVersion">Logical Director release recorded in the <c>imap</c> payload.</param>
    protected BlLegacyFileWriterBase(BlTag codecTag, bool isBigEndian, BlLegacyDirectorVersion directorVersion)
    {
        if (directorVersion == BlLegacyDirectorVersion.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(directorVersion), directorVersion, "Director version must be specified.");
        }

        _codecTag = codecTag;
        _endianness = isBigEndian ? BlEndianness.BigEndian : BlEndianness.LittleEndian;
        _mapVersion = directorVersion.ToMapVersion();
        _archiveVersion = directorVersion.ToArchiveVersionMarker();
        _signature = isBigEndian ? BlTag.RIFX.Value : BlTag.XFIR.Value;
    }

    /// <summary>
    /// Writes the supplied container to the provided stream using the configured Director header.
    /// </summary>
    /// <param name="stream">Destination stream that receives the movie bytes.</param>
    /// <param name="container">DTO container describing the resources to embed.</param>
    public void Write(Stream stream, DirFilesContainerDTO container)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(container);

        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
        }

        var descriptors = BuildDescriptors(container);
        using var payloadStream = BuildPayload(descriptors);

        var payloadLength = payloadStream.Length;
        if (payloadLength > uint.MaxValue)
        {
            throw new InvalidOperationException("Director containers cannot exceed 4 GB of payload data.");
        }

        stream.Position = 0;
        stream.SetLength(0);

        var headerWriter = new BlStreamWriter(stream)
        {
            Endianness = _endianness
        };

        headerWriter.WriteAscii(_signature);
        headerWriter.WriteUInt32((uint)payloadLength);
        headerWriter.WriteTag(_codecTag);
        headerWriter.Flush();

        payloadStream.Position = 0;
        payloadStream.CopyTo(stream);
        stream.Flush();
    }

    private List<ResourceDescriptor> BuildDescriptors(DirFilesContainerDTO container)
    {
        var descriptors = new List<ResourceDescriptor>(container.Files.Count);

        for (int i = 0; i < container.Files.Count; i++)
        {
            var resource = container.Files[i];
            if (resource is null)
            {
                throw new InvalidOperationException("Resource entries cannot be null.");
            }

            string baseName = ExtractBaseName(resource.FileName);
            var tag = ParseTag(baseName, resource.FileName);
            int id = ParseResourceId(baseName, i);
            var bytes = resource.Bytes ?? Array.Empty<byte>();

            if (bytes.LongLength > uint.MaxValue)
            {
                throw new InvalidOperationException($"Resource '{resource.FileName}' exceeds the maximum supported size.");
            }

            descriptors.Add(new ResourceDescriptor(tag, id, i, bytes));
        }

        descriptors.Sort(CompareDescriptors);
        return descriptors;
    }

    private MemoryStream BuildPayload(List<ResourceDescriptor> descriptors)
    {
        const uint ImapPayloadLength = 16;
        const ushort MmapHeaderSize = 12;
        const ushort MmapEntrySize = 20;

        var stream = new MemoryStream();
        var writer = new BlStreamWriter(stream)
        {
            Endianness = _endianness
        };

        writer.WriteTag(BlTag.Imap);
        writer.WriteUInt32(ImapPayloadLength);
        writer.WriteUInt32(ImapPayloadLength);
        writer.WriteUInt32(_mapVersion);

        long mapOffsetPatchPosition = writer.Position;
        writer.WriteUInt32(0);
        writer.WriteUInt32(_archiveVersion);

        long mmapChunkStart = writer.Position;
        long entryCount = descriptors.Count;
        long mmapPayloadLengthLong = MmapHeaderSize + entryCount * MmapEntrySize;
        uint mmapPayloadLength = CheckedToUInt32(mmapPayloadLengthLong, "mmap payload length");

        writer.WriteTag(BlTag.Mmap);
        writer.WriteUInt32(mmapPayloadLength);
        writer.WriteUInt16(MmapHeaderSize);
        writer.WriteUInt16(MmapEntrySize);
        writer.WriteUInt32((uint)entryCount);
        writer.WriteUInt32((uint)entryCount);

        long resourceDataStartRelative = mmapChunkStart + 8 + mmapPayloadLength;
        long resourceDataStartAbsoluteLong = resourceDataStartRelative + HeaderLength;
        uint currentOffset = CheckedToUInt32(resourceDataStartAbsoluteLong, "resource data start");
        foreach (var descriptor in descriptors)
        {
            descriptor.Offset = currentOffset;
            uint paddedPayload = AlignToEven(descriptor.PayloadLength);
            uint chunkTotal = CheckedAdd(paddedPayload, 8u, "resource chunk length");
            currentOffset = CheckedAdd(currentOffset, chunkTotal, "resource offsets");
        }

        foreach (var descriptor in descriptors)
        {
            writer.WriteTag(descriptor.Tag);
            writer.WriteUInt32(descriptor.PayloadLength);
            writer.WriteUInt32(descriptor.Offset);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            writer.WriteUInt32(uint.MaxValue);
        }

        writer.Position = resourceDataStartRelative;

        foreach (var descriptor in descriptors)
        {
            writer.WriteTag(descriptor.Tag);
            writer.WriteUInt32(descriptor.PayloadLength);
            writer.WriteBytes(descriptor.Data);
            if ((descriptor.PayloadLength & 1) != 0)
            {
                writer.WriteByte(0);
            }
        }

        writer.Position = mapOffsetPatchPosition;
        var mapOffsetAbsoluteLong = mmapChunkStart + HeaderLength;
        writer.WriteUInt32(CheckedToUInt32(mapOffsetAbsoluteLong, "mmap offset"));

        writer.Position = writer.Length;
        return stream;
    }

    private static string ExtractBaseName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Resource entries must provide a file name that begins with the resource tag.");
        }

        var baseName = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrEmpty(baseName) || baseName.Length < 4)
        {
            throw new InvalidOperationException($"Resource file name '{fileName}' must begin with a four-character tag.");
        }

        return baseName;
    }

    private static BlTag ParseTag(string baseName, string originalName)
    {
        string tagText = baseName.Substring(0, 4);
        if (!BlTag.TryParse(tagText, out var tag))
        {
            throw new InvalidOperationException($"Resource file name '{originalName}' must start with a valid four-character tag.");
        }

        return tag;
    }

    private static int ParseResourceId(string baseName, int fallbackId)
    {
        int separatorIndex = baseName.IndexOf('_');
        if (separatorIndex < 0 || separatorIndex >= baseName.Length - 1)
        {
            return fallbackId;
        }

        var span = baseName.AsSpan(separatorIndex + 1);
        int length = 0;
        while (length < span.Length && char.IsDigit(span[length]))
        {
            length++;
        }

        if (length == 0)
        {
            return fallbackId;
        }

        var digits = span.Slice(0, length);
        if (int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return id;
        }

        return fallbackId;
    }

    private static int CompareDescriptors(ResourceDescriptor left, ResourceDescriptor right)
    {
        int idCompare = left.Id.CompareTo(right.Id);
        if (idCompare != 0)
        {
            return idCompare;
        }

        int tagCompare = string.CompareOrdinal(left.Tag.Value, right.Tag.Value);
        if (tagCompare != 0)
        {
            return tagCompare;
        }

        return left.Order.CompareTo(right.Order);
    }

    private static uint AlignToEven(uint value) => (value & 1) == 0 ? value : value + 1;

    private static uint CheckedToUInt32(long value, string description)
    {
        if (value < 0 || value > uint.MaxValue)
        {
            throw new InvalidOperationException($"The {description} exceeds the maximum supported size for Director archives.");
        }

        return (uint)value;
    }

    private static uint CheckedAdd(uint left, uint right, string description)
    {
        ulong sum = (ulong)left + right;
        if (sum > uint.MaxValue)
        {
            throw new InvalidOperationException($"The {description} exceeds the maximum supported size for Director archives.");
        }

        return (uint)sum;
    }

    private sealed class ResourceDescriptor
    {
        public ResourceDescriptor(BlTag tag, int id, int order, byte[] data)
        {
            Tag = tag;
            Id = id;
            Order = order;
            Data = data;
            PayloadLength = (uint)data.Length;
        }

        public BlTag Tag { get; }

        public int Id { get; }

        public int Order { get; }

        public byte[] Data { get; }

        public uint PayloadLength { get; }

        public uint Offset { get; set; }
    }
}
