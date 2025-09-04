using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    public void Visit(LingoCallNode node)
    {
        if (node.Callee is LingoVarNode varNode)
        {
            if (varNode.VarName.Equals("voidp", StringComparison.OrdinalIgnoreCase))
            {
                node.Arguments.Accept(this);
                Append(" == null");
                return;
            }
            if (varNode.VarName.Equals("objectp", StringComparison.OrdinalIgnoreCase))
            {
                node.Arguments.Accept(this);
                Append(" != null");
                return;
            }
        }

        if (!string.IsNullOrEmpty(node.Name))
        {
            if (!string.IsNullOrEmpty(node.TargetType))
            {
                var param = node.TargetType.ToLowerInvariant();
                Append("CallMovieScript<");
                Append(node.TargetType);
                Append(">(");
                Append($"{param} => {param}.");
                Append(node.Name);
                Append("());");
                AppendLine();
            }
            else
            {
                AppendLine($"{node.Name}();");
            }
            return;
        }

        if (node.Callee is LingoVarNode func)
        {
            var name = func.VarName.ToLowerInvariant();
            List<LingoNode>? args;
            if (node.Arguments is LingoDatumNode dn &&
                dn.Datum.Type == LingoDatum.DatumType.ArgList &&
                dn.Datum.Value is List<LingoNode> list)
            {
                args = list;
            }
            else
            {
                args = new List<LingoNode> { node.Arguments };
            }

            switch (name)
            {
                case "append" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "add" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "addat" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".AddAt(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "addprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteAt(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "getat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetAt(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "setat" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetAt(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "count" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Count");
                    return;
                case "deleteone" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteOne(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteall" when args.Count >= 1:
                    args[0].Accept(this);
                    AppendLine(".DeleteAll();");
                    return;
                case "duplicate" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Duplicate()");
                    return;
                case "getone" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetOne()");
                    return;
                case "getlast" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetLast()");
                    return;
                case "getavalue" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetAValue()");
                    return;
                case "ilk" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Ilk()");
                    return;
                case "listp" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".ListP()");
                    return;
                case "tolist" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".ToList()");
                    return;
                case "sort" when args.Count >= 1:
                    args[0].Accept(this);
                    if (args.Count >= 2)
                    {
                        Append(".Sort(");
                        args[1].Accept(this);
                        AppendLine(");");
                    }
                    else
                    {
                        AppendLine(".Sort();");
                    }
                    return;
                case "setprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetProp(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteProp(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "getprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetProp(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getaprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetaProp(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getaprop" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetAProp()");
                    return;
                case "getpropat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetPropAt(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "setaprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetaProp(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "setaprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".SetAProp(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "findpos" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".FindPos(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "findposnear" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".FindPosNear(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getpos" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetPos(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "max" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Max()");
                    return;
                case "min" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Min()");
                    return;
                case "value" when args.Count >= 1:
                    Append("Convert.ToInt32(");
                    args[0].Accept(this);
                    Append(")");
                    return;
            }
        }

        WriteCallExpr(node);
        AppendLine(";");
    }

    private void WriteCallExpr(LingoCallNode node)
    {
        if (node.Callee is LingoObjPropExprNode prop &&
            prop.Property is LingoVarNode { VarName: "new" } &&
            prop.Object is LingoCallNode { Callee: LingoVarNode { VarName: "script" }, Arguments: LingoDatumNode dn } &&
            dn.Datum.Type == LingoDatum.DatumType.String)
        {
            var scriptName = dn.Datum.AsString();
            if (_scriptTypes.TryGetValue(scriptName, out var st))
            {
                var suffix = st switch
                {
                    LingoScriptType.Movie => "MovieScript",
                    LingoScriptType.Parent => "Parent",
                    LingoScriptType.Behavior => "Behavior",
                    _ => "Script"
                };
                var cls = SanitizeIdentifier(scriptName) + suffix;
                Append("new ");
                Append(cls);
                Append("(_env, _globalvars");
                if (node.Arguments is not LingoBlockNode)
                {
                    Append(", ");
                    node.Arguments.Accept(this);
                }
                Append(")");
                return;
            }
        }

        if (node.Callee is LingoVarNode func)
        {
            var name = func.VarName;
            if (name.Equals("castLib", StringComparison.OrdinalIgnoreCase))
                Append("CastLib");
            else if (name.Equals("sprite", StringComparison.OrdinalIgnoreCase))
                Append("Sprite");
            else
                Append(name);
        }
        else
        {
            node.Callee.Accept(this);
        }
        Append("(");
        node.Arguments.Accept(this);
        Append(")");
    }

    private void WriteObjCallExpr(LingoObjCallNode node)
    {
        var name = node.Name.Value.AsString();
        if (name.Equals("castLib", StringComparison.OrdinalIgnoreCase))
            Append("CastLib");
        else
            Append(name);
        Append("(");
        node.ArgList.Accept(this);
        Append(")");
    }

    private void WriteObjCallV4Expr(LingoObjCallV4Node node)
    {
        var methodName = node.Name.Value.AsString();

        // script("Foo").new(args)
        if (methodName.Equals("new", StringComparison.OrdinalIgnoreCase) &&
            node.Object is LingoCallNode call &&
            call.Callee is LingoVarNode { VarName: "script" } &&
            call.Arguments is LingoDatumNode dn &&
            dn.Datum.Type == LingoDatum.DatumType.String)
        {
            var scriptName = dn.Datum.AsString();
            if (_scriptTypes.TryGetValue(scriptName, out var st))
            {
                var suffix = st switch
                {
                    LingoScriptType.Movie => "MovieScript",
                    LingoScriptType.Parent => "Parent",
                    LingoScriptType.Behavior => "Behavior",
                    _ => "Script"
                };
                var cls = SanitizeIdentifier(scriptName) + suffix;
                Append("new ");
                Append(cls);
                Append("(_env, _globalvars");
                bool hasArgs = node.ArgList.Datum.Type == LingoDatum.DatumType.ArgList &&
                               node.ArgList.Datum.Value is List<LingoNode> list && list.Count > 0;
                if (hasArgs)
                {
                    Append(", ");
                    node.ArgList.Accept(this);
                }
                Append(")");
                return;
            }
        }

        var startLen = _sb.Length;
        if (node.Object is LingoCallNode callObj)
            WriteCallExpr(callObj);
        else if (node.Object is LingoObjCallNode objCall)
            WriteObjCallExpr(objCall);
        else
            node.Object.Accept(this);
        TrimSemicolon(startLen);

        Append(".");
        var lower = methodName.ToLowerInvariant();
        var pascal = lower switch
        {
            "deleteone" => "DeleteOne",
            "getpos" => "GetPos",
            "deleteat" => "DeleteAt",
            "getat" => "GetAt",
            "setat" => "SetAt",
            "count" => "Count",
            "add" => "Add",
            "addat" => "AddAt",
            "addprop" => "Add",
            _ => methodName
        };
        Append(pascal);
        Append("(");
        node.ArgList.Accept(this);
        Append(")");
    }
}

