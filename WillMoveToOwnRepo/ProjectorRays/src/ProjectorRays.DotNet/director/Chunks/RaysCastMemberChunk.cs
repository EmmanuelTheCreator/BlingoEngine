using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Text;
using System.Xml.Linq;

namespace ProjectorRays.director.Chunks;

public class RaysCastMemberChunk : RaysChunk
{
    public RaysMemberType Type;
    public uint InfoLen;
    public uint SpecificDataLen;
    public RaysCastInfoChunk? Info;
    public BufferView SpecificData;
    public RaysCastMember? Member;
    public bool HasFlags1;
    public byte Flags1;
    public ushort Id;
    public RaysScriptChunk? Script;

    public RaysCastMemberTextRead DecodedText { get; internal set; }
    public BufferView InfoView { get; private set; }

    public RaysCastMemberChunk(RaysDirectorFile? dir) : base(dir, ChunkType.CastMemberChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        //Dir.Logger.LogInformation($"Reading CastMemberChunk at stream.Pos={stream.Pos}");

        //var raw = stream.ReadByteView(12);
        //Dir.Logger.LogInformation("Raw CastMember header bytes: " + BitConverter.ToString(raw.Data, raw.Offset, raw.Size));

        //stream.Seek(stream.Pos - 12); // rewind
        // Respect the movie's byte order for configuration data as well.
        //stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        Type = (RaysMemberType)stream.ReadUint32();
        InfoLen = stream.ReadUint32();
        SpecificDataLen = stream.ReadUint32();
        Dir.Logger.LogTrace($"CastMember InfoLen={InfoLen}, SpecificDataLen={SpecificDataLen}, Stream.Pos={stream.Pos}, Stream.Size={stream.Size}");
        InfoView = stream.ReadByteView((int)InfoLen);
        Dir.Logger.LogInformation("InfoView:\r\n" + InfoView.LogHex());
        if (InfoLen > 0)
        {
            var infoStream = new ReadStream(InfoView, stream.Endianness);
            Info = new RaysCastInfoChunk(Dir);
            Info.ReadV2(infoStream);
        }
        HasFlags1 = false;
        SpecificData = stream.ReadByteView((int)SpecificDataLen);
        //Dir.Logger.LogInformation("SpecificData:\r\n" + SpecificData.LogHex());
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("type", Type.ToString());
        json.WriteField("id", Id);
        if (Info != null)
        {
            json.WriteKey("info");
            Info.WriteJSON(json);
        }
        json.EndObject();
    }
    public override void LogInfo(StringBuilder sb, int indentation)
    {
        sb.AppendLine($"{new string(' ', indentation)}---------------------------");
        base.LogInfo(sb, indentation);
        sb.AppendLine($"{new string(' ', indentation)}type: {Type}");
        sb.AppendLine($"{new string(' ', indentation)}Id: {Id}");
        if (Info != null) Info.LogInfo(sb, indentation + 2);
        sb.AppendLine($"{new string(' ', indentation)}SpecificDataLen: {SpecificDataLen}");
        sb.AppendLine($"{new string(' ', indentation)}SpecificData (as text): '{GetText()}'");
        sb.AppendLine($"{new string(' ', indentation)}-----------");
        if (Script != null) Script.LogInfo(sb, indentation + 2);

        sb.AppendLine($"{SpecificData.LogHex()}");
        sb.AppendLine($"{new string(' ', indentation)}---------------------------");
    }

    public uint GetScriptID() => Info?.ScriptId ?? 0;
    public string GetScriptText() => Info?.ScriptSrcText ?? string.Empty;
    public void SetScriptText(string val) { if (Info != null) Info.ScriptSrcText = val; }
    public string GetName() => Info?.Name ?? string.Empty;
    public string GetText()
    {
        if (!string.IsNullOrEmpty(DecodedText?.Text))
            return DecodedText.Text;

        if (Type == RaysMemberType.TextMember || Type == RaysMemberType.FieldMember)
        {
            var span = SpecificData.Data.AsSpan(SpecificData.Offset, SpecificData.Size);

            if (span.Length > 1 && span[0] <= span.Length - 1)
                return Encoding.Latin1.GetString(span.Slice(1, span[0]));

            int len = span.IndexOf((byte)0);
            if (len < 0) len = span.Length;

            return Encoding.Latin1.GetString(span.Slice(0, len));
        }

        return string.Empty;
    }
}
