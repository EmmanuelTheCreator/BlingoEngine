using ProjectorRays.Common;
using ProjectorRays.LingoDec;
using ProjectorRays.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.director.Chunks;
using ProjectorRays.director;
using ProjectorRays.director.Scores;
using System.Collections.Generic;
using System.Text;

namespace ProjectorRays.Director;

public class ChunkInfo
{
    public int Id;
    public uint FourCC;
    public uint Len;
    public uint UncompressedLen;
    public int Offset;
    public RayGuid CompressionID;
}

public class RaysDirectorFile : ChunkResolver
{
    private RaysDataBlockReader _raysDataBlockReader;
    public RaysKeyTableChunk? KeyTable { get; set; }
    public RaysConfigChunk? Config { get; set; }
    public List<RaysCastChunk> Casts { get; set; } = new();
    public RaysScoreChunk? Score { get; set; }
    public ILogger Logger { get; set; }
    public string Name { get; set; }
    public bool DotSyntax { get; private set; }
    public uint Version => _raysDataBlockReader.Version;
    public Endianness Endianness => _raysDataBlockReader.Endianness;
    public IReadOnlyDictionary<int, ChunkInfo> ChunkInfos => _raysDataBlockReader.ChunkInfoMap;

    public static uint FOURCC(char a, char b, char c, char d)
        => ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | (uint)d;

    public RaysDirectorFile(ILogger logger, string name = "")
    {
        Logger = logger; Name = name;
        _raysDataBlockReader = new RaysDataBlockReader(this);
    }

    /// <summary>
    /// Entry point for loading a Director movie. Depending on the codec this
    /// will either read the standard memory map or the Afterburner tables.
    /// Returns <c>true</c> on success.
    /// </summary>
    public virtual bool Read(ReadStream stream, bool parseChunks = true, bool parseScore = true)
    {
        //var rawBytes = stream.ReadByteView(stream.Size);
        //Logger.LogInformation.WriteLine("Raw CASt chunk bytes: " + BitConverter.ToString(rawBytes.Data, rawBytes.Offset, rawBytes.Size));
        //stream.Seek(0); // reset
        //Stream = stream;

        (bool flowControl, bool value) = _raysDataBlockReader.Read(stream);
        if (!flowControl)
        {
            return value;
        }

        if (parseChunks)
        {
            if (!ReadKeyTable()) return false;
            if (!ReadConfig()) return false;
            if (!ReadCasts()) return false;
            if (parseScore)
            {
                ReadScore();
            }
        }

        return true;
    }




    private bool ReadKeyTable()
    {
        KeyTable = _raysDataBlockReader.GetKeyTable(); // GetFirstChunkInfo(FOURCC('K','E','Y','*'));
        return KeyTable != null;
    }

    private bool ReadConfig()
    {
        Config = _raysDataBlockReader.GetConfig();
        if (Config == null) return false;
        _raysDataBlockReader.Version = RaysUtilities.HumanVersion((uint)Config.DirectorVersion);
        DotSyntax = _raysDataBlockReader.Version >= 700;
        return true;
    }

    private bool ReadCasts()
    {
        bool internalCast = true;
        if (_raysDataBlockReader.Version >= 500)
        {
            var info = _raysDataBlockReader.GetFirstMCsL(); // GetFirstChunkInfo(FOURCC('M','C','s','L'));
            if (info != null)
            {
                var castList = (RaysCastListChunk)GetChunk(info.FourCC, info.Id);
                foreach (var entry in castList.Entries)
                {
                    int sectionID = -1;
                    foreach (var keyEntry in KeyTable!.Entries)
                        if (keyEntry.CastID == entry.Id && keyEntry.FourCC == FOURCC('C', 'A', 'S', '*'))
                        { sectionID = keyEntry.SectionID; break; }
                    if (sectionID > 0)
                    {
                        var cast = _raysDataBlockReader.GetCastStar(sectionID)!;
                        cast.Populate(entry.Name, entry.Id, entry.MinMember);
                        Casts.Add(cast);
                    }
                }
                if (Casts.Count > 0)
                    return true;
            }
            else
            {
                internalCast = false;
            }
        }
        //var info5 = ChunkInfoMap[7];
        //Logger.LogTrace($"CAS* Compression: {info5.CompressionID}");
        var def = _raysDataBlockReader.GetFirstCast(); // GetFirstChunkInfo(FOURCC('C','A','S','*'));
        //if (def != null)
        //{
        //    var cast = (CastChunk)GetChunk(def.FourCC, def.Id);
        //    cast.Populate(internalCast ? "Internal" : "External", 1024, (ushort) Config!.MinMember);
        //    Casts.Add(cast);
        //}
        if (def != null && def.FourCC == FOURCC('C', 'A', 'S', '*'))
        {
            // You expect a CastChunk, but chunk type must actually be 'CAS*'
            var cast = (RaysCastChunk)GetChunk(FOURCC('C', 'A', 'S', '*'), def.Id);
            cast.Populate(internalCast ? "Internal" : "External", 1024, (ushort)Config!.MinMember);
            var sb = new StringBuilder();
            cast.LogInfo(sb, 2);
            Logger.LogInformation(sb.ToString());
            Casts.Add(cast);
        }

        return true;
    }

    /// <summary>
    /// Attempt to load the VWSC score chunk if present. Failure is ignored
    /// as some files may omit this information.
    /// </summary>
    private void ReadScore()
    {
        var info = _raysDataBlockReader.GetFirstScore(); // .GetFirstChunkInfo(FOURCC('V','W','S','C'));
        if (info != null)
        {
            Score = (RaysScoreChunk)GetChunk(info.FourCC, info.Id);
        }
    }
    public bool ChunkExists(uint fourCC, int id) => _raysDataBlockReader.ChunkExists(fourCC, id);

    public ChunkInfo? GetChunkOrDefault(int id) => _raysDataBlockReader.GetChunkOrDefault(id);
    public RaysCastMemberChunk? GetCastMember(int id) => _raysDataBlockReader.GetCastMember(id);
    public RaysChunk GetChunk(uint fourCC, int id) => _raysDataBlockReader.GetChunk(fourCC, id);
    public RaysScript? GetScript(int id) => _raysDataBlockReader.GetScript(id)?.Script;
    public RaysScriptChunk? GetScriptChunk(int id) => _raysDataBlockReader.GetScript(id);
    public ScriptNames? GetScriptNames(int id) => _raysDataBlockReader.GetScriptNames(id)?.Names;
    public ChunkInfo? GetFirstXMED() => _raysDataBlockReader.GetFirstXMED();
    public ChunkInfo? GetLastXMED() => _raysDataBlockReader.GetLastXMED();
    internal ChunkInfo? TryGetChunkTest(uint fourCC) => _raysDataBlockReader.TryGetChunkTest(fourCC);
    /// <summary>
    /// Parse all scripts referenced by every cast so the bytecode is converted
    /// into a minimal AST. This mirrors the C++ <c>parseScripts()</c> helper.
    /// </summary>
    public void ParseScripts()
    {
        foreach (var cast in Casts)
        {
            cast.Lctx?.Context.ParseScripts();
        }
    }

    /// <summary>
    /// Restore the source text for script members by decompiling the compiled
    /// bytecode and storing it back into the cast member info structures.
    /// </summary>
    public void RestoreScriptText()
    {
        foreach (var cast in Casts)
        {
            var ctx = cast.Lctx?.Context;
            if (ctx == null) continue;

            foreach (var pair in ctx.Scripts)
            {
                uint id = pair.Key;
                var script = pair.Value;
                if (!ChunkExists(FOURCC('L', 's', 'c', 'r'), (int)id))
                    continue;
                var scriptChunk = (RaysScriptChunk)GetChunk(FOURCC('L', 's', 'c', 'r'), (int)id);
                var member = scriptChunk.Member;
                if (member != null)
                {
                    member.SetScriptText(script.ScriptText(RaysFileIO.PlatformLineEnding, DotSyntax));
                }
            }
        }
    }


}
