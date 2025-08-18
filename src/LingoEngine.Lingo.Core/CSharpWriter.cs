using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Writes a small subset of Lingo AST to C# using the visitor pattern.
/// Only a handful of nodes are supported for now.
/// </summary>
public class CSharpWriter : ILingoAstVisitor
{
    private readonly StringBuilder _sb = new();
    private readonly string _methodAccessModifier;
    private string? _currentHandlerName;
    private int _indent;
    private bool _atLineStart = true;

    public CSharpWriter(string methodAccessModifier = "public")
    {
        _methodAccessModifier = methodAccessModifier;
    }

    /// <summary>Converts the given AST node to C#.</summary>
    public static string Write(LingoNode node, string methodAccessModifier = "public")
    {
        var writer = new CSharpWriter(methodAccessModifier);
        node.Accept(writer);
        return writer._sb.ToString();
    }

    /// <summary>Fallback helper for token sequences.</summary>
    public static string WriteTokens(IEnumerable<LingoToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var tok in tokens)
        {
            switch (tok.Type)
            {
                case LingoTokenType.Put:
                    sb.Append("// put\n");
                    break;
                case LingoTokenType.Into:
                    sb.Append("// into\n");
                    break;
                default:
                    sb.Append(tok.Lexeme + " ");
                    break;
            }
        }
        return sb.ToString();
    }

    private void WriteIndent()
    {
        if (_atLineStart)
        {
            for (int i = 0; i < _indent; i++)
                _sb.Append("    ");
            _atLineStart = false;
        }
    }

    private void Append(string text)
    {
        if (text.Length == 0) return;
        WriteIndent();
        _sb.Append(text);
    }

    private void AppendLine(string text)
    {
        WriteIndent();
        _sb.AppendLine(text);
        _atLineStart = true;
    }

    private void AppendLine()
    {
        _sb.AppendLine();
        _atLineStart = true;
    }

    private void Indent() => _indent++;
    private void Unindent() { if (_indent > 0) _indent--; }

    private static string DatumToCSharp(LingoDatum datum)
    {
        return datum.Type switch
        {
            LingoDatum.DatumType.Integer or LingoDatum.DatumType.Float => datum.AsString(),
            LingoDatum.DatumType.String => $"\"{datum.AsString()}\"",
            LingoDatum.DatumType.Symbol or LingoDatum.DatumType.VarRef => datum.AsString(),
            LingoDatum.DatumType.List => datum.Value is List<LingoNode> list
                ? "[" + string.Join(", ", list.Select(n => Write(n).Trim())) + "]"
                : "[]",
            _ => datum.AsString()
        };
    }

    private void Unsupported(LingoNode node)
    {
        // Unhandled nodes are ignored for now
    }

    public void Visit(LingoHandlerNode node)
    {
        var prevHandler = _currentHandlerName;
        var name = node.Handler?.Name ?? string.Empty;
        _currentHandlerName = name;
        if (name.Length > 0)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];
            Append(_methodAccessModifier);
            Append(" void ");
            Append(name);
            Append("(");
            var args = node.Handler.ArgumentNames
                .Where(a => !a.Equals("me", StringComparison.OrdinalIgnoreCase))
                .Select(a => $"object {a}");
            Append(string.Join(", ", args));
            AppendLine(")");
            AppendLine("{");
            Indent();
            node.Block.Accept(this);
            Unindent();
            AppendLine("}");
            AppendLine();
        }
        else
        {
            node.Block.Accept(this);
        }
        _currentHandlerName = prevHandler;
    }

    public void Visit(LingoErrorNode node) => AppendLine("// error");

    public void Visit(LingoCommentNode node)
    {
        Append("// ");
        Append(node.Text);
        AppendLine();
    }

    public void Visit(LingoNewObjNode node)
    {
        Append("new ");
        Append(node.ObjType);
        Append("(");
        node.ObjArgs.Accept(this);
        Append(")");
    }

    public void Visit(LingoLiteralNode node) => Append(node.Value.ToString());

    public void Visit(LingoIfStmtNode node)
    {
        Append("if (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.ThenBlock.Accept(this);
        Unindent();
        AppendLine("}");
        if (node.HasElse)
        {
            if (node.ElseBlock is LingoBlockNode block && block.Children.Count == 1 && block.Children[0] is LingoIfStmtNode nested)
            {
                Append("else ");
                nested.Accept(this);
            }
            else
            {
                AppendLine("else");
                AppendLine("{");
                Indent();
                node.ElseBlock!.Accept(this);
                Unindent();
                AppendLine("}");
            }
        }
    }

    public void Visit(LingoIfElseStmtNode node)
    {
        Append("if (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.ThenBlock.Accept(this);
        Unindent();
        AppendLine("}");
        AppendLine("else");
        AppendLine("{");
        Indent();
        node.ElseBlock.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoEndCaseNode node) => AppendLine("// end case");

    public void Visit(LingoObjCallNode node)
    {
        Append(node.Name.Value.AsString());
        Append("(");
        node.ArgList.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoPutStmtNode node)
    {
        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoTheExprNode node)
    {
        var propLower = node.Prop.ToLowerInvariant();
        if (propLower.StartsWith("mouse"))
        {
            Append("_Mouse.");
            Append(char.ToUpperInvariant(node.Prop[0]) + node.Prop.Substring(1));
        }
        else if (propLower == "actorlist")
        {
            Append("_Movie.ActorList");
        }
        else if (propLower == "frame")
        {
            Append("_Movie.CurrentFrame");
        }
        else
        {
            Append($"/* the {node.Prop} */");
        }
    }

    public void Visit(LingoBinaryOpNode node)
    {
        bool needsParensLeft = node.Left is LingoBinaryOpNode;
        bool needsParensRight = node.Right is LingoBinaryOpNode;

        if (needsParensLeft) Append("(");
        node.Left.Accept(this);
        if (needsParensLeft) Append(")");

        Append(" ");
        Append(BinaryOpcodeToString(node.Opcode));
        Append(" ");

        if (needsParensRight) Append("(");
        node.Right.Accept(this);
        if (needsParensRight) Append(")");
    }

    private static string BinaryOpcodeToString(LingoBinaryOpcode opcode)
    {
        return opcode switch
        {
            LingoBinaryOpcode.Add => "+",
            LingoBinaryOpcode.Subtract => "-",
            LingoBinaryOpcode.Multiply => "*",
            LingoBinaryOpcode.Divide => "/",
            LingoBinaryOpcode.Modulo => "%",
            LingoBinaryOpcode.And => "&&",
            LingoBinaryOpcode.Or => "||",
            LingoBinaryOpcode.Concat => "+",
            LingoBinaryOpcode.Equals => "==",
            LingoBinaryOpcode.NotEquals => "!=",
            LingoBinaryOpcode.GreaterThan => ">",
            LingoBinaryOpcode.GreaterOrEqual => ">=",
            LingoBinaryOpcode.LessThan => "<",
            LingoBinaryOpcode.LessOrEqual => "<=",
            _ => opcode.ToString()
        };
    }

    public void Visit(LingoCaseStmtNode node)
    {
        Append("switch (");
        node.Value.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        var label = node.FirstLabel as LingoCaseLabelNode;
        while (label != null)
        {
            Append("case ");
            label.Value.Accept(this);
            AppendLine(":");
            Indent();
            label.Block?.Accept(this);
            Unindent();
            label = label.NextLabel;
        }
        if (node.Otherwise != null)
        {
            AppendLine("default:");
            Indent();
            node.Otherwise.Accept(this);
            Unindent();
        }
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoExitStmtNode node) => AppendLine("return;");

    public void Visit(LingoReturnStmtNode node)
    {
        if (node.Value is LingoVarNode varNode &&
            varNode.VarName.Equals("me", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_currentHandlerName, "new", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Append("return");
        if (node.Value != null)
        {
            Append(" ");
            node.Value.Accept(this);
        }
        AppendLine(";");
    }

    public void Visit(LingoTellStmtNode node)
    {
        AppendLine("// tell block");
        node.Block.Accept(this);
    }

    public void Visit(LingoWhenStmtNode node) => AppendLine("// when statement");

    public void Visit(LingoOtherwiseNode node) => node.Block.Accept(this);

    public void Visit(LingoCaseLabelNode node)
    {
        Append("case ");
        node.Value.Accept(this);
        AppendLine(":");
        Indent();
        node.Block?.Accept(this);
        Unindent();
    }

    public void Visit(LingoChunkExprNode node) => node.Expr.Accept(this);

    public void Visit(LingoInverseOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoObjCallV4Node node)
    {
        if (node.Object is LingoVarNode objVar &&
            objVar.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            var name = node.Name.Value.AsString();
            Append(char.ToUpperInvariant(name[0]) + name[1..]);
            Append("(");
            node.ArgList.Accept(this);
            AppendLine(");");
            return;
        }

        node.Object.Accept(this);
        Append(".");
        Append(node.Name.Value.AsString());
        Append("(");
        node.ArgList.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoMemberExprNode node)
    {
        Append("Member(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoObjPropExprNode node)
    {
        if (node.Object is LingoVarNode objVar &&
            objVar.VarName.Equals("me", StringComparison.OrdinalIgnoreCase) &&
            node.Property is LingoVarNode propVar)
        {
            Append(char.ToUpperInvariant(propVar.VarName[0]) + propVar.VarName[1..]);
            return;
        }

        if (node.Object is LingoTheExprNode theNode)
        {
            node.Object.Accept(this);
            Append(".");
            if (node.Property is LingoVarNode propVar2)
            {
                var name = propVar2.VarName;
                if (theNode.Prop.Equals("actorlist", StringComparison.OrdinalIgnoreCase) &&
                    name.Equals("append", StringComparison.OrdinalIgnoreCase))
                {
                    Append("Add");
                }
                else
                {
                    Append(char.ToUpperInvariant(name[0]) + name[1..]);
                }
            }
            else
            {
                node.Property.Accept(this);
            }
        }
        else
        {
            node.Object.Accept(this);
            Append(".");
            node.Property.Accept(this);
        }
    }

    public void Visit(LingoPlayCmdStmtNode node)
    {
        Append("Play(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoThePropExprNode node)
    {
        Append("TheProp(");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(LingoMenuPropExprNode node)
    {
        Append("MenuProp(");
        node.Menu.Accept(this);
        Append(", ");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(LingoSoundCmdStmtNode node)
    {
        Append("Sound(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoSoundPropExprNode node)
    {
        Append("Sound(");
        node.Sound.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(LingoCursorStmtNode node)
    {
        Append("Cursor = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoGoToStmtNode node)
    {
        Append("_Movie.GoTo(");
        node.Target.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoAssignmentStmtNode node)
    {
        if (node.Target is LingoVarNode lhs && lhs.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            bool rhsIsVoid = node.Value is LingoVarNode rhsVar && rhsVar.VarName.Equals("void", StringComparison.OrdinalIgnoreCase)
                || node.Value is LingoDatumNode rhsDatum && rhsDatum.Datum.Type == LingoDatum.DatumType.Void;

            if (rhsIsVoid)
                return;
        }

        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoSendSpriteStmtNode node)
    {
        Append("SendSprite");
        string param = "sprite";
        if (!string.IsNullOrEmpty(node.TargetType))
        {
            Append("<");
            Append(node.TargetType);
            Append(">");
            param = node.TargetType.ToLowerInvariant();
        }
        Append("(");
        node.Sprite.Accept(this);
        Append($", {param} => {param}.");
        if (node.Message is LingoDatumNode dn && dn.Datum.Type == LingoDatum.DatumType.Symbol)
            Append(dn.Datum.AsSymbol());
        else
            node.Message.Accept(this);
        Append("());");
        AppendLine();
    }

    public void Visit(LingoExitRepeatStmtNode node) => AppendLine("break;");

    public void Visit(LingoNextRepeatStmtNode node) => AppendLine("continue;");

    public void Visit(LingoObjBracketExprNode node)
    {
        node.Object.Accept(this);
        Append("[");
        node.Index.Accept(this);
        Append("]");
    }

    public void Visit(LingoSpritePropExprNode node)
    {
        Append("Sprite(");
        node.Sprite.Accept(this);
        Append(").");
        if (node.Property is LingoVarNode propVar)
            Append(char.ToUpperInvariant(propVar.VarName[0]) + propVar.VarName[1..]);
        else
            node.Property.Accept(this);
    }

    public void Visit(LingoChunkDeleteStmtNode node)
    {
        Append("DeleteChunk(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoChunkHiliteStmtNode node)
    {
        Append("Hilite(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoGlobalDeclStmtNode node) { }

    public void Visit(LingoPropertyDeclStmtNode node) { }

    public void Visit(LingoInstanceDeclStmtNode node) { }

    public void Visit(LingoRepeatWhileStmtNode node)
    {
        Append("while (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoMenuItemPropExprNode node)
    {
        Append("menuItem(");
        node.MenuItem.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(LingoObjPropIndexExprNode node)
    {
        node.Object.Accept(this);
        Append(".prop[");
        node.PropertyIndex.Accept(this);
        Append("]");
    }

    public void Visit(LingoRepeatWithInStmtNode node)
    {
        Append($"foreach (var {node.Variable} in ");
        node.List.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoRepeatWithToStmtNode node)
    {
        Append($"for (var {node.Variable} = ");
        node.Start.Accept(this);
        Append($"; {node.Variable} <= ");
        node.End.Accept(this);
        Append($"; {node.Variable}++)");
        AppendLine();
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoSpriteWithinExprNode node)
    {
        Append("SpriteWithin(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(LingoLastStringChunkExprNode node)
    {
        Append("LastChunkOf(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(LingoSpriteIntersectsExprNode node)
    {
        Append("SpriteIntersects(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(LingoStringChunkCountExprNode node)
    {
        Append("ChunkCount(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(LingoNotOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoCallNode node)
    {
        if (node.Callee is LingoVarNode varNode && varNode.VarName.Equals("voidp", StringComparison.OrdinalIgnoreCase))
        {
            node.Arguments.Accept(this);
            Append(" == null");
            return;
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

        node.Callee.Accept(this);
        Append("(");
        node.Arguments.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoVarNode node)
    {
        if (node.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
            Append("this");
        else
            Append(node.VarName);
    }

    public void Visit(LingoBlockNode node)
    {
        foreach (var child in node.Children)
        {
            child.Accept(this);
        }
    }

    public void Visit(LingoDatumNode datumNode)
    {
        Append(DatumToCSharp(datumNode.Datum));
    }

    public void Visit(LingoRepeatWithStmtNode repeatWithStmtNode)
    {
        Append($"for (var {repeatWithStmtNode.Variable} = ");
        repeatWithStmtNode.Start.Accept(this);
        Append($"; {repeatWithStmtNode.Variable} <= ");
        repeatWithStmtNode.End.Accept(this);
        Append($"; {repeatWithStmtNode.Variable}++)");
        AppendLine();
        AppendLine("{");
        Indent();
        repeatWithStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoRepeatUntilStmtNode repeatUntilStmtNode)
    {
        AppendLine("do");
        AppendLine("{");
        Indent();
        repeatUntilStmtNode.Body.Accept(this);
        Unindent();
        Append("} while (!(");
        repeatUntilStmtNode.Condition.Accept(this);
        AppendLine("));");
    }

    public void Visit(LingoRepeatForeverStmtNode repeatForeverStmtNode)
    {
        AppendLine("while (true)");
        AppendLine("{");
        Indent();
        repeatForeverStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoRepeatTimesStmtNode repeatTimesStmtNode)
    {
        Append("for (int i = 1; i <= ");
        repeatTimesStmtNode.Count.Accept(this);
        AppendLine("; i++)");
        AppendLine("{");
        Indent();
        repeatTimesStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(LingoExitRepeatIfStmtNode exitRepeatIfStmtNode)
    {
        Append("if (");
        exitRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") break;");
    }

    public void Visit(LingoNextRepeatIfStmtNode nextRepeatIfStmtNode)
    {
        Append("if (");
        nextRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") continue;");
    }

    public void Visit(LingoNextStmtNode nextStmtNode) => AppendLine("// next");
}
