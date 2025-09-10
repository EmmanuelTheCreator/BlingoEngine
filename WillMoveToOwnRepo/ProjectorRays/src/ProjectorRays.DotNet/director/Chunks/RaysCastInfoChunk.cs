using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Text;

namespace ProjectorRays.director.Chunks;

public class RaysCastInfoChunk : RaysListChunk
{
    public uint Flags;
    public uint ScriptId;
    public string ScriptSrcText = string.Empty;
    public string Name = string.Empty;

    public RaysCastInfoChunk(RaysDirectorFile? dir) : base(dir, ChunkType.CastInfoChunk)
    {
        Writable = true;
    }

    public override void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        Flags = stream.ReadUint32();
        ScriptId = stream.ReadUint32();
    }

    public override void Read(ReadStream stream)
    {
        base.Read(stream);
        if (OffsetTableLen > 0 && Items.Count >= 1)
            ScriptSrcText = ReadString(0);
        if (OffsetTableLen > 1 && Items.Count >= 2)
            Name = ReadPascalString(1);
        if (OffsetTableLen == 0)
        {
            // Ensure there is one entry so decompilation results can be stored
            OffsetTableLen = 1;
            OffsetTable.Add(0);
        }
    }
    public void ReadV2(ReadStream stream)
    {
        base.Read(stream);
        stream.ReadBytes((int)(DataOffset * 4)+2);
        var val1 = stream.ReadInt32();
        var val2 = stream.ReadInt32();
        var val3 = stream.ReadInt32();
        var nameLength = stream.ReadInt8();
        Name = stream.ReadString(nameLength);
        var val4 = stream.ReadInt16();
        var val5 = stream.ReadInt16();
        var val6 = stream.ReadInt16();
        var val6b = stream.ReadInt16();
        var val7 = stream.ReadInt32();
        var val8 = stream.ReadInt32();
        var val9 = stream.ReadInt16();
        var val10 = stream.ReadInt16();
        var val11 = stream.ReadInt16();
        var restData = stream.ReadBytes(10);
        // 9F 94 68 51 9F
        // C6 4E            OR | A5 4E
        // 2F 41 00 


        //if (OffsetTableLen > 0 && Items.Count >= 1)
        //    ScriptSrcText = ReadString(0);
        //if (OffsetTableLen > 1 && Items.Count >= 2)
        //    Name = ReadPascalString(1);
        //if (OffsetTableLen == 0)
        //{
        //    // Ensure there is one entry so decompilation results can be stored
        //    OffsetTableLen = 1;
        //    OffsetTable.Add(0);
        //}
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("dataOffset", DataOffset);
        json.WriteField("flags", Flags);
        json.WriteField("scriptId", ScriptId);
        json.WriteField("scriptSrcText", ScriptSrcText);
        json.WriteField("name", Name);
        json.EndObject();
    }
    public override void LogInfo(StringBuilder sb, int indentation)
    {
        base.LogInfo(sb, indentation);
        sb.AppendLine($"{new string(' ', indentation)}Name: '{Name}'");
        sb.AppendLine($"{new string(' ', indentation)}DataOffset: {DataOffset}");
        sb.AppendLine($"{new string(' ', indentation)}Flags: {Flags}");
        sb.AppendLine($"{new string(' ', indentation)}ScriptId: {ScriptId}");
        sb.AppendLine($"{new string(' ', indentation)}ScriptSrcText: '{ScriptSrcText}'");
    }
}
