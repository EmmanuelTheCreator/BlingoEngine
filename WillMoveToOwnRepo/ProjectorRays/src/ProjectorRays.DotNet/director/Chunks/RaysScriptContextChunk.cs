using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.BlingoDec;
using System.Text;

namespace ProjectorRays.director.Chunks;

public class RaysScriptContextChunk : RaysChunk
{
    public ScriptContext Context;

    public RaysScriptContextChunk(RaysDirectorFile? dir) : base(dir, ChunkType.ScriptContextChunk)
    {
        Context = new ScriptContext(dir?.Version ?? 0, dir!);
    }

    public override void Read(ReadStream stream)
    {
        Context.Read(stream);
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("entryCount", Context.EntryCount);
        json.WriteField("flags", Context.Flags);
        json.EndObject();
    }
    public override void LogInfo(StringBuilder sb, int indentation)
    {
        base.LogInfo(sb, indentation);
        sb.AppendLine($"{new string(' ', indentation)}EntryCount: {Context.EntryCount}");
        sb.AppendLine($"{new string(' ', indentation)}Flags: 0x{Context.Flags:X4}");
        foreach (var entry in Context.Scripts)
        {
            sb.AppendLine($"{new string(' ', indentation+2)}ParentNumber: {entry.Value.ParentNumber}");
            sb.AppendLine($"{new string(' ', indentation+2)}ScriptNumber: {entry.Value.ScriptNumber}");
            sb.AppendLine($"{new string(' ', indentation+2)}CastID: {entry.Value.CastID}");
            sb.AppendLine($"{new string(' ', indentation+2)}FactoryName: {entry.Value.FactoryName}");
            sb.AppendLine($"{new string(' ', indentation+2)}Version: {entry.Value.Version}");
            sb.AppendLine($"{new string(' ', indentation+2)}GlobalNames: {string.Join(',', entry.Value.GlobalNames)}");
            sb.AppendLine($"{new string(' ', indentation+2)}PropertyNames: {string.Join(',', entry.Value.PropertyNames)}");
            sb.AppendLine($"{new string(' ', indentation+2)}Handlers: {string.Join(',', entry.Value.Handlers.Select(x => x.Name+"("+string.Join(',',x.ArgumentNames)+")"))}");
            sb.AppendLine($"{new string(' ', indentation+2)}Literals: {string.Join(',', entry.Value.Literals.Select(x => x.Type+"|"+x.Value))}");
        }
    }

}

