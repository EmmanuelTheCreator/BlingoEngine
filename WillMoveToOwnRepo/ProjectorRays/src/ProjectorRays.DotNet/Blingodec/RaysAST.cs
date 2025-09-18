using ProjectorRays.Common;
using System.Collections.Generic;
using System.Linq;

namespace ProjectorRays.BlingoDec;

public class Datum
{
    public DatumType Type;
    public int I;
    public double F;
    public string S = string.Empty;
    public List<Datum> L = new();

    public Datum() { Type = DatumType.kDatumVoid; }
    public Datum(int val) { Type = DatumType.kDatumInt; I = val; }
    public Datum(double val) { Type = DatumType.kDatumFloat; F = val; }
    public Datum(DatumType t, string val) { Type = t; S = val; }

    public Datum Clone()
    {
        var copy = new Datum
        {
            Type = Type,
            I = I,
            F = F,
            S = S,
            L = L.Select(item => item.Clone()).ToList()
        };
        return copy;
    }

    public int ToInt()
    {
        return Type switch
        {
            DatumType.kDatumInt => I,
            DatumType.kDatumFloat => (int)F,
            _ => 0
        };
    }

    public void WriteScriptText(RaysCodeWriter code, bool dot, bool sum)
    {
        switch (Type)
        {
            case DatumType.kDatumVoid:
                code.Write("VOID");
                break;
            case DatumType.kDatumSymbol:
                code.Write("#" + S);
                break;
            case DatumType.kDatumVarRef:
                code.Write(S);
                break;
            case DatumType.kDatumString:
                if (S.Length == 0)
                    code.Write("EMPTY");
                else
                    code.Write('"' + S + '"');
                break;
            case DatumType.kDatumInt:
                code.Write(I.ToString());
                break;
            case DatumType.kDatumFloat:
                code.Write(RaysUtil.FloatToString(F));
                break;
            default:
                code.Write("LIST");
                break;
        }
    }
}

public abstract class AstNode
{
    /// <summary>
    /// Write a textual representation of this node. This is a very minimal
    /// pretty printer used by <see cref="RaysScript.WriteScriptText"/> and does not
    /// attempt to cover all Lingo syntax features.
    /// </summary>
    public virtual void WriteScriptText(RaysCodeWriter code) { }
    public virtual bool HasSpaces() => false;
    public virtual Datum? GetValue() => null;
}

public class BlockNode : AstNode
{
    public List<AstNode> Statements { get; } = new();

    public override void WriteScriptText(RaysCodeWriter code)
    {
        foreach (var stmt in Statements)
        {
            stmt.WriteScriptText(code);
            code.WriteLine();
        }
    }
}

public class LiteralNode : AstNode
{
    public Datum Value;
    public LiteralNode(Datum value) => Value = value;

    public override void WriteScriptText(RaysCodeWriter code) =>
        Value.WriteScriptText(code, false, false);
    public override Datum? GetValue() => Value;
}

public class VarNode : AstNode
{
    public string Name;
    public VarNode(string name) => Name = name;

    public override void WriteScriptText(RaysCodeWriter code) => code.Write(Name);
}

public class BinaryOpNode : AstNode
{
    public OpCode Op;
    public AstNode Left;
    public AstNode Right;
    public BinaryOpNode(OpCode op, AstNode left, AstNode right)
    {
        Op = op;
        Left = left;
        Right = right;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        Left.WriteScriptText(code);
        code.Write(" ");
        code.Write(StandardNames.GetName(StandardNames.BinaryOpNames, (uint)Op));
        code.Write(" ");
        Right.WriteScriptText(code);
    }
    public override bool HasSpaces() => true;
}

public class UnaryOpNode : AstNode
{
    public OpCode Op;
    public AstNode Operand;
    public UnaryOpNode(OpCode op, AstNode operand)
    {
        Op = op;
        Operand = operand;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        string opStr = Op == OpCode.kOpNot ? "not" : "-";
        code.Write(opStr + " ");
        Operand.WriteScriptText(code);
    }
    public override bool HasSpaces() => true;
}

public class AssignmentNode : AstNode
{
    public AstNode Target;
    public AstNode Value;
    public AssignmentNode(AstNode target, AstNode value)
    {
        Target = target;
        Value = value;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        Target.WriteScriptText(code);
        code.Write(" = ");
        Value.WriteScriptText(code);
    }
    public override bool HasSpaces() => true;
}

public class ListNode : AstNode
{
    public List<AstNode> Items { get; }
    public bool IsPropertyList { get; }

    public ListNode(List<AstNode> items, bool isPropertyList)
    {
        Items = items;
        IsPropertyList = isPropertyList;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write("[");
        if (IsPropertyList)
        {
            for (int i = 0; i < Items.Count; i += 2)
            {
                if (i > 0)
                    code.Write(", ");
                var key = Items[i];
                key.WriteScriptText(code);
                if (i + 1 < Items.Count)
                {
                    code.Write(": ");
                    Items[i + 1].WriteScriptText(code);
                }
            }
        }
        else
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (i > 0)
                    code.Write(", ");
                Items[i].WriteScriptText(code);
            }
        }
        code.Write("]");
    }

    public override bool HasSpaces() => Items.Count > 1 || Items.Any(item => item.HasSpaces());
}

public class PropertyAccessNode : AstNode
{
    public AstNode Target;
    public string Property;
    public PropertyAccessNode(AstNode target, string property)
    {
        Target = target;
        Property = property;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        bool paren = Target.HasSpaces();
        if (paren)
            code.Write("(");
        Target.WriteScriptText(code);
        if (paren)
            code.Write(")");
        code.Write(".");
        code.Write(Property);
    }

    public override bool HasSpaces() => false;
}

public class MemberAccessNode : AstNode
{
    public string Prefix;
    public AstNode MemberId;
    public AstNode? CastId;

    public MemberAccessNode(string prefix, AstNode memberId, AstNode? castId)
    {
        Prefix = prefix;
        MemberId = memberId;
        CastId = castId;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write(Prefix);
        code.Write("(");
        MemberId.WriteScriptText(code);
        if (CastId != null)
        {
            code.Write(", ");
            CastId.WriteScriptText(code);
        }
        code.Write(")");
    }

    public override bool HasSpaces() => true;
}

public class PutNode : AstNode
{
    public AstNode Target;
    public AstNode Value;
    public PutType Type;

    public PutNode(AstNode target, AstNode value, PutType type)
    {
        Target = target;
        Value = value;
        Type = type;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write("put ");
        Value.WriteScriptText(code);
        code.Write(Type switch
        {
            PutType.kPutAfter => " after ",
            PutType.kPutBefore => " before ",
            _ => " into "
        });
        Target.WriteScriptText(code);
    }

    public override bool HasSpaces() => true;
}

public class DeleteNode : AstNode
{
    public AstNode Target;
    public DeleteNode(AstNode target) => Target = target;

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write("delete ");
        Target.WriteScriptText(code);
    }

    public override bool HasSpaces() => true;
}

public class TheNode : AstNode
{
    public string Property;
    public TheNode(string property) => Property = property;

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write("the ");
        code.Write(Property);
    }

    public override bool HasSpaces() => true;
}

public class ArgListNode : AstNode
{
    public List<AstNode> Args;
    public bool NoReturn;
    public ArgListNode(List<AstNode> args, bool noReturn)
    {
        Args = args;
        NoReturn = noReturn;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        for (int i = 0; i < Args.Count; i++)
        {
            if (i > 0) code.Write(", ");
            Args[i].WriteScriptText(code);
        }
    }
    public override bool HasSpaces() => Args.Count > 1 || Args.Any(a => a.HasSpaces());
}

public class CallNode : AstNode
{
    public string Name;
    public List<AstNode> Args;
    public bool Statement;
    public CallNode(string name, List<AstNode> args, bool statement)
    {
        Name = name;
        Args = args;
        Statement = statement;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write(Name);
        code.Write("(");
        for (int i = 0; i < Args.Count; i++)
        {
            if (i > 0) code.Write(", ");
            Args[i].WriteScriptText(code);
        }
        code.Write(")");
    }
    public override bool HasSpaces() => Args.Count > 1 || Args.Any(a => a.HasSpaces());
}

public class ReturnNode : AstNode
{
    public AstNode? Value;
    public ReturnNode(AstNode? value) => Value = value;

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write("return");
        if (Value != null)
        {
            code.Write(" ");
            Value.WriteScriptText(code);
        }
    }
    public override bool HasSpaces() => true;
}

public class ChunkExprNode : AstNode
{
    public ChunkExprType Type;
    public AstNode First;
    public AstNode Last;
    public AstNode String;
    public ChunkExprNode(ChunkExprType type, AstNode first, AstNode last, AstNode str)
    {
        Type = type;
        First = first;
        Last = last;
        String = str;
    }

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write(StandardNames.GetName(StandardNames.ChunkTypeNames, (uint)Type));
        code.Write(" ");
        bool parenFirst = First.HasSpaces();
        if (parenFirst) code.Write("(");
        First.WriteScriptText(code);
        if (parenFirst) code.Write(")");
        if (!(Last is LiteralNode lit && lit.Value.Type == DatumType.kDatumInt && lit.Value.I == 0))
        {
            code.Write(" to ");
            bool parenLast = Last.HasSpaces();
            if (parenLast) code.Write("(");
            Last.WriteScriptText(code);
            if (parenLast) code.Write(")");
        }
        code.Write(" of ");
        bool stringIsBiggerChunk = String is ChunkExprNode c && c.Type > this.Type;
        bool parenString = !stringIsBiggerChunk && String.HasSpaces();
        if (parenString) code.Write("(");
        String.WriteScriptText(code);
        if (parenString) code.Write(")");
    }

    public override bool HasSpaces() => true;
}

public class UnknownOpNode : AstNode
{
    public Bytecode Bytecode;
    public UnknownOpNode(Bytecode bc) => Bytecode = bc;

    public override void WriteScriptText(RaysCodeWriter code)
    {
        code.Write($"/* unknown {Bytecode.Opcode} */");
    }
}


