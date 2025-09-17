using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using System.Text;

namespace ProjectorRays.director
{
    public class RaysDataBlockReader
    {
        private int _ilsBodyOffset;
        private byte[] _ilsBuf = Array.Empty<byte>();

        private readonly Dictionary<int, byte[]> _cachedChunkBufs = new();
        private readonly Dictionary<int, BufferView> _cachedChunkViews = new();
        public ReadStream? Stream;
        public Endianness Endianness;
        public string FverVersionString = string.Empty;
        public uint Codec;
        public bool Afterburned;

        public Dictionary<uint, List<int>> ChunkIDsByFourCC = new();
        public Dictionary<int, ChunkInfo> ChunkInfoMap = new();
        public Dictionary<int, RaysChunk> DeserializedChunks = new();

        public RaysInitialMapChunk? InitialMap;
        public RaysMemoryMapChunk? MemoryMap;
        public ILogger Logger;
        public RaysDirectorFile Dir { get; private set; }
        public uint Version;

        public RaysDataBlockReader(RaysDirectorFile directorFile)
        {
            Dir = directorFile;
            Logger = Dir.Logger;
        }


        public static uint FOURCC(char a, char b, char c, char d)
            => ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | (uint)d;
        public (bool flowControl, bool value) Read(ReadStream stream)
        {
            Stream = stream;
            stream.Endianness = Endianness.BigEndian;
            uint metaFourCC = stream.ReadUint32();
            if (metaFourCC == FOURCC('X', 'F', 'I', 'R'))
                stream.Endianness = Endianness.LittleEndian;
            Endianness = stream.Endianness;
            Logger.LogInformation($"File endianness: {Endianness}");



            stream.ReadUint32(); // meta length
            Codec = stream.ReadUint32();

            if (Codec == FOURCC('M', 'V', '9', '3') || Codec == FOURCC('M', 'C', '9', '5'))
            {
                ReadMemoryMap();
            }
            else if (Codec == FOURCC('F', 'G', 'D', 'M') || Codec == FOURCC('F', 'G', 'D', 'C'))
            {
                Afterburned = true;
                if (!ReadAfterburnerMap())
                    return (flowControl: false, value: false);
            }
            else
            {
                // unsupported codec
                return (flowControl: false, value: false);
            }

            return (flowControl: true, value: default);
        }
        /// <summary>
        /// Load the standard movie memory map. This resolves the initial and
        /// memory map chunks so subsequent chunk lookups know their offsets.
        /// </summary>
        private void ReadMemoryMap()
        {
            InitialMap = (RaysInitialMapChunk)ReadChunk(FOURCC('i', 'm', 'a', 'p'));
            DeserializedChunks[1] = InitialMap;

            Stream!.Seek((int)InitialMap.MmapOffset);
            MemoryMap = (RaysMemoryMapChunk)ReadChunk(FOURCC('m', 'm', 'a', 'p'));
            DeserializedChunks[2] = MemoryMap;

            for (int i = 0; i < MemoryMap.MapArray.Count; i++)
            {
                var entry = MemoryMap.MapArray[i];
                if (entry.FourCC == FOURCC('f', 'r', 'e', 'e') || entry.FourCC == FOURCC('j', 'u', 'n', 'k'))
                    continue;

                ChunkInfo info = new()
                {
                    Id = i,
                    FourCC = entry.FourCC,
                    Len = entry.Len,
                    UncompressedLen = entry.Len,
                    Offset = entry.Offset,
                    CompressionID = BlingoGuidConstants.NULL_COMPRESSION_GUID
                };
                AddChunkInfo(info);
                Logger.LogInformation($"Registering chunk {Common.RaysUtil.FourCCToString(info.FourCC)} with ID {info.Id}");
            }
        }

        /// <summary>
        /// Parse the Afterburner resource table used by protected movies. This
        /// mirrors the C++ implementation and decodes the zlib-compressed mapping
        /// tables describing every chunk in the file.
        /// </summary>
        private bool ReadAfterburnerMap()
        {
            var s = Stream!;
            if (s.ReadUint32() != FOURCC('F', 'v', 'e', 'r')) return false;
            uint fverLength = s.ReadVarInt();
            int start = s.Pos;
            uint fverVersion = s.ReadVarInt();
            if (fverVersion >= 0x401)
            {
                s.ReadVarInt();
                s.ReadVarInt();
            }
            if (fverVersion >= 0x501)
            {
                byte len = s.ReadUint8();
                FverVersionString = s.ReadString(len);
            }
            int end = s.Pos;
            if (end - start != fverLength)
                s.Seek(start + (int)fverLength);

            if (s.ReadUint32() != FOURCC('F', 'c', 'd', 'r')) return false;
            uint fcdrLength = s.ReadVarInt();
            byte[] fcdrBuf = new byte[fcdrLength * 10];
            int fcdrUncomp = s.ReadZlibBytes((int)fcdrLength, fcdrBuf, fcdrBuf.Length);
            if (fcdrUncomp <= 0) return false;
            var fcdrStream = new ReadStream(fcdrBuf, fcdrUncomp, Endianness);
            ushort compCount = fcdrStream.ReadUint16();
            var compIDs = new List<RayGuid>();
            for (int i = 0; i < compCount; i++)
            {
                var id = new RayGuid();
                id.Read(fcdrStream);
                compIDs.Add(id);
            }
            for (int i = 0; i < compCount; i++)
                fcdrStream.ReadCString();

            if (s.ReadUint32() != FOURCC('A', 'B', 'M', 'P')) return false;
            uint abmpLength = s.ReadVarInt();
            int abmpEnd = s.Pos + (int)abmpLength;
            uint abmpCompressionType = s.ReadVarInt();
            uint abmpUncompLen = s.ReadVarInt();
            byte[] abmpBuf = new byte[abmpUncompLen];
            int abmpActual = s.ReadZlibBytes(abmpEnd - s.Pos, abmpBuf, abmpBuf.Length);
            if (abmpActual <= 0) return false;
            var abmpStream = new ReadStream(abmpBuf, abmpActual, Endianness);

            abmpStream.ReadVarInt();
            abmpStream.ReadVarInt();
            uint resCount = abmpStream.ReadVarInt();
            for (int i = 0; i < resCount; i++)
            {
                int resId = (int)abmpStream.ReadVarInt();
                int offset = (int)abmpStream.ReadVarInt();
                uint compSize = abmpStream.ReadVarInt();
                uint uncompSize = abmpStream.ReadVarInt();
                uint compType = abmpStream.ReadVarInt();
                uint tag = abmpStream.ReadUint32();

                ChunkInfo info = new()
                {
                    Id = resId,
                    FourCC = tag,
                    Len = compSize,
                    UncompressedLen = uncompSize,
                    Offset = offset,
                    CompressionID = compIDs[(int)compType]
                };
                AddChunkInfo(info);
            }

            if (!ChunkInfoMap.ContainsKey(2)) return false;
            if (s.ReadUint32() != FOURCC('F', 'G', 'E', 'I')) return false;
            var ilsInfo = ChunkInfoMap[2];
            s.ReadVarInt();
            _ilsBodyOffset = s.Pos;
            _ilsBuf = new byte[ilsInfo.UncompressedLen];
            int ilsActual = s.ReadZlibBytes((int)ilsInfo.Len, _ilsBuf, _ilsBuf.Length);
            var ilsStream = new ReadStream(_ilsBuf, ilsActual, Endianness);
            while (!ilsStream.Eof)
            {
                int resId = (int)ilsStream.ReadVarInt();
                var info = ChunkInfoMap[resId];
                _cachedChunkViews[resId] = ilsStream.ReadByteView((int)info.Len);
            }

            return true;
        }

        private void AddChunkInfo(ChunkInfo info)
        {
            ChunkInfoMap[info.Id] = info;
            if (!ChunkIDsByFourCC.TryGetValue(info.FourCC, out var list))
            {
                list = new List<int>();
                ChunkIDsByFourCC[info.FourCC] = list;
            }
            list.Add(info.Id);
        }

        private RaysChunk ReadChunk(uint fourCC, uint len = uint.MaxValue)
        {
            BufferView view = ReadChunkData(fourCC, len);
            return MakeChunk(fourCC, view);
        }

        private BufferView ReadChunkData(uint fourCC, uint len)
        {
            var s = Stream!;
            int offset = s.Pos;
            uint validFourCC = s.ReadUint32();
            uint validLen = s.ReadUint32();
            if (len == uint.MaxValue) len = validLen;
            if (fourCC != validFourCC || len != validLen)
                throw new IOException($"At offset {offset} expected '{Common.RaysUtil.FourCCToString(fourCC)}' chunk len {len} but got '{Common.RaysUtil.FourCCToString(validFourCC)}' len {validLen}");
            return s.ReadByteView((int)len);
        }
        private RaysChunk MakeChunk(uint fourCC, BufferView view)
        {
            //Logger.LogInformation($"Reading chunk FourCC={ProjectorRays.Common.Util.FourCCToString(fourCC)}, Offset={view.Offset}, Length={view.Size}");
            //Logger.LogInformation($"Chunk bytes:\n{view.LogHex(256,view.Size)}");

            RaysChunk chunk = fourCC switch
            {
                var v when v == FOURCC('i', 'm', 'a', 'p') => new RaysInitialMapChunk(Dir),
                var v when v == FOURCC('m', 'm', 'a', 'p') => new RaysMemoryMapChunk(Dir),
                var v when v == FOURCC('C', 'A', 'S', '*') => new RaysCastChunk(Dir),
                var v when v == FOURCC('C', 'A', 'S', 't') => new RaysCastMemberChunk(Dir),
                var v when v == FOURCC('K', 'E', 'Y', '*') => new RaysKeyTableChunk(Dir),
                var v when v == FOURCC('L', 'c', 't', 'x') || v == FOURCC('L', 'c', 't', 'X') => new RaysScriptContextChunk(Dir),
                var v when v == FOURCC('L', 'n', 'a', 'm') => new RaysScriptNamesChunk(Dir),
                var v when v == FOURCC('L', 's', 'c', 'r') => new RaysScriptChunk(Dir),
                var v when v == FOURCC('V', 'W', 'C', 'F') || v == FOURCC('D', 'R', 'C', 'F') => new RaysConfigChunk(Dir),
                var v when v == FOURCC('M', 'C', 's', 'L') => new RaysCastListChunk(Dir),
                var v when v == FOURCC('V', 'W', 'S', 'C') => new RaysScoreChunk(Dir),
                var v when v == FOURCC('X', 'M', 'E', 'D') => new RaysXmedChunk(Dir, ChunkType.StyledText),
                var v when v == FOURCC('M', 'i', 'd', 'e') => new RaysBinaryChunk(Dir),
                var v when v == FOURCC('e', 'd', 'i', 'M') => new RaysBinaryChunk(Dir),
                var v when v == FOURCC('B', 'I', 'T', 'D') => new RaysBinaryChunk(Dir),
                var v when v == FOURCC('A', 'L', 'F', 'A') => new RaysBinaryChunk(Dir),
                var v when v == FOURCC('T', 'h', 'u', 'm') => new RaysBinaryChunk(Dir),
                _ => throw new IOException($"Could not deserialize '{Common.RaysUtil.FourCCToString(fourCC)}' chunk")
            };


            var isBig = IsAlwaysBigEndian(fourCC);
            if (isBig)
            {

            }
            var chunkStream = new ReadStream(view, isBig ? Endianness.BigEndian : Endianness);
            chunk.Read(chunkStream);

            return chunk;
        }

        private static bool IsAlwaysBigEndian(uint fourCC) => fourCC switch
        {
            var v when v == FOURCC('C', 'A', 'S', '*') => true,
            var v when v == FOURCC('C', 'A', 'S', 't') => true,
            var v when v == FOURCC('M', 'C', 's', 'L') => true,
            _ => false
        };
        public T? TryGetChunk<T>(uint fourCC, int id) where T : RaysChunk => ChunkExists(fourCC, id) ? (T)GetChunk(fourCC, id) : default;
        public RaysChunk GetChunk(uint fourCC, int id)
        {
            if (DeserializedChunks.TryGetValue(id, out var chunk))
                return chunk;

            BufferView view = GetChunkData(fourCC, id);
            chunk = MakeChunk(fourCC, view);
            DeserializedChunks[id] = chunk;
            if (chunk.ChunkType != ChunkType.CastChunk)
            {
                var sb = new StringBuilder();
                chunk.LogInfo(sb, 2);
                Logger.LogInformation(sb.ToString());
            }
            return chunk;
        }
        public bool ChunkExists(uint fourCC, int id) => ChunkInfoMap.ContainsKey(id) && ChunkInfoMap[id].FourCC == fourCC;
        public ChunkInfo? GetChunkOrDefault(int id) => ChunkInfoMap.GetValueOrDefault(id);
        public ChunkInfo? GetFirstChunkInfo(uint fourCC)
        {
            if (ChunkIDsByFourCC.TryGetValue(fourCC, out var list) && list.Count > 0)
                return ChunkInfoMap[list[0]];
            return null;
        }
        public ChunkInfo? GetLastChunkInfo(uint fourCC)
        {
            if (ChunkIDsByFourCC.TryGetValue(fourCC, out var list) && list.Count > 0)
                return ChunkInfoMap[list[^1]];
            return null;
        }
        internal ChunkInfo? TryGetChunkTest(uint fourCC)
        {
            if (MemoryMap == null) return null;
            // MemoryMapEntry is a struct, so cannot be compared to null.
            // Use a flag to check if a valid entry was found.
            var found = false;
            MemoryMapEntry mapEntry = default;
            foreach (var entry in MemoryMap.MapArray)
            {
                if (entry.FourCC == fourCC)
                {
                    mapEntry = entry;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                //var info = ChunkInfoMap.Values.FirstOrDefault(c => c.FourCC == fourCC && c.Id == mapEntry.Id);
                //return info;
            }
            return null;
        }



        public RaysScoreChunk? ReadScore()
        {
            var info = GetFirstChunkInfo(FOURCC('V', 'W', 'S', 'C'));
            if (info != null)
                return (RaysScoreChunk)GetChunk(info.FourCC, info.Id);
            return null;
        }
        public RaysScriptChunk? GetScript(int id) => TryGetChunk<RaysScriptChunk>(FOURCC('L', 's', 'c', 'r'), id);
        public RaysScriptNamesChunk? GetScriptNames(int id) => TryGetChunk<RaysScriptNamesChunk>(FOURCC('L', 'n', 'a', 'm'), id);
        public RaysCastChunk? GetCastStar(int id) => TryGetChunk<RaysCastChunk>(FOURCC('C', 'A', 'S', '*'), id);
        public RaysCastMemberChunk? GetCastMember(int id) => TryGetChunk<RaysCastMemberChunk>(FOURCC('C', 'A', 'S', 't'), id);
        public ChunkInfo? GetFirstCast() => GetFirstChunkInfo(FOURCC('C', 'A', 'S', '*'));
        public ChunkInfo? GetFirstMCsL() => GetFirstChunkInfo(FOURCC('M', 'C', 's', 'L'));
        public ChunkInfo? GetFirstScore() => GetFirstChunkInfo(FOURCC('V', 'W', 'S', 'C'));
        public ChunkInfo? GetFirstKeyTable() => GetFirstChunkInfo(FOURCC('K', 'E', 'Y', '*'));
        public ChunkInfo? GetFirstXMED() => GetFirstChunkInfo(FOURCC('X', 'M', 'E', 'D'));
        public ChunkInfo? GetLastXMED() => GetLastChunkInfo(FOURCC('X', 'M', 'E', 'D'));
        public RaysKeyTableChunk? GetKeyTable()
        {
            var info = GetFirstKeyTable(); // GetFirstChunkInfo(FOURCC('K','E','Y','*'));
            if (info == null) return null;
            var keyTable = (RaysKeyTableChunk)GetChunk(info.FourCC, info.Id);
            return keyTable;
        }
        public RaysConfigChunk? GetConfig()
        {
            var info = GetFirstChunkInfo(FOURCC('D', 'R', 'C', 'F')) ?? GetFirstChunkInfo(FOURCC('V', 'W', 'C', 'F'));
            if (info == null) return null;
            return (RaysConfigChunk)GetChunk(info.FourCC, info.Id);
        }
        private BufferView GetChunkData(uint fourCC, int id)
        {
            if (!ChunkInfoMap.TryGetValue(id, out var info))
                throw new IOException($"Could not find chunk {id}");
            if (fourCC != info.FourCC)
                throw new IOException($"Expected chunk {id} to be '{Common.RaysUtil.FourCCToString(fourCC)}' but is '{Common.RaysUtil.FourCCToString(info.FourCC)}'");

            if (_cachedChunkViews.TryGetValue(id, out var view))
                return view;

            var s = Stream!;
            if (Afterburned)
            {
                s.Seek(info.Offset + _ilsBodyOffset);
                if (info.Len == 0 && info.UncompressedLen == 0)
                    _cachedChunkViews[id] = s.ReadByteView((int)info.Len);
                else if (CompressionImplemented(info.CompressionID))
                {
                    int actual = -1;
                    _cachedChunkBufs[id] = new byte[info.UncompressedLen];
                    if (info.CompressionID.Equals(BlingoGuidConstants.ZLIB_COMPRESSION_GUID))
                        actual = s.ReadZlibBytes((int)info.Len, _cachedChunkBufs[id], _cachedChunkBufs[id].Length);
                    else if (info.CompressionID.Equals(BlingoGuidConstants.SND_COMPRESSION_GUID))
                    {
                        // Sound decompression not implemented
                        BufferView chunkView = s.ReadByteView((int)info.Len);
                        _cachedChunkViews[id] = chunkView;
                        actual = chunkView.Size;
                    }
                    if (actual < 0) throw new IOException($"Chunk {id}: Could not decompress");
                    _cachedChunkViews[id] = new BufferView(_cachedChunkBufs[id], actual);
                }
                else if (info.CompressionID.Equals(BlingoGuidConstants.FONTMAP_COMPRESSION_GUID))
                {
                    _cachedChunkViews[id] = RaysFontMap.GetFontMap((int)Version);
                }
                else
                {
                    _cachedChunkViews[id] = s.ReadByteView((int)info.Len);
                }
            }
            else
            {
                s.Seek(info.Offset);
                _cachedChunkViews[id] = ReadChunkData(fourCC, info.Len);
            }

            return _cachedChunkViews[id];
        }

        private static bool CompressionImplemented(RayGuid id) =>
        id.Equals(BlingoGuidConstants.ZLIB_COMPRESSION_GUID) || id.Equals(BlingoGuidConstants.SND_COMPRESSION_GUID);


    }
}

