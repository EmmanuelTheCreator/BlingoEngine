using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    private static string DatumToCSharp(LingoDatum datum)
    {
        return datum.Type switch
        {
            LingoDatum.DatumType.Integer or LingoDatum.DatumType.Float => datum.AsString(),
            LingoDatum.DatumType.String => $"\"{EscapeString(datum.AsString())}\"",
            LingoDatum.DatumType.Symbol => $"Symbol(\"{datum.AsString()}\")",
            LingoDatum.DatumType.VarRef => datum.AsString(),
            LingoDatum.DatumType.List => datum.Value is List<LingoNode> list
                ? "[" + string.Join(", ", list.Select(n => Write(n).Trim())) + "]"
                : "[]",
            LingoDatum.DatumType.ArgList or LingoDatum.DatumType.ArgListNoRet => datum.Value is List<LingoNode> argList
                ? string.Join(", ", argList.Select(n => Write(n).Trim()))
                : string.Empty,
            LingoDatum.DatumType.PropList => datum.Value is List<LingoNode> propList
                ? "new LingoPropertyList<object?>" +
                    (propList.Count == 0
                        ? "()"
                        : " { " + string.Join(", ", Enumerable.Range(0, propList.Count / 2)
                            .Select(i => $"[{Write(propList[2 * i]).Trim()}] = {Write(propList[2 * i + 1]).Trim()}")) + " }")
                : "new LingoPropertyList<object?>()",
            _ => datum.AsString()
        };
    }

    private static string EscapeString(string s) => s
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n");
}

