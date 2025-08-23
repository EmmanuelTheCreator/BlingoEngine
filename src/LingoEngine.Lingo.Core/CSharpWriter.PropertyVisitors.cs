using System;
using System.Linq;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    public void Visit(LingoObjPropExprNode node)
    {
        if (node.Property is LingoVarNode prop && prop.VarName.Equals("charToNum", StringComparison.OrdinalIgnoreCase))
        {
            node.Object.Accept(this);
            Append(".CharToNum()");
            return;
        }

        if (node.Object is LingoMemberExprNode member &&
            node.Property is LingoVarNode propVarText)
        {
            var propName = propVarText.VarName;
            if (propName.Equals("text", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("line", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("word", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                Append("GetMember<ILingoMemberTextBase>(");
                member.Expr.Accept(this);
                if (member.CastLib != null)
                {
                    Append(", ");
                    member.CastLib.Accept(this);
                }
                Append(").");
                Append(char.ToUpperInvariant(propName[0]) + propName[1..]);
                return;
            }
        }

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
                var lower = propVar2.VarName.ToLowerInvariant();
                var pascal = lower switch
                {
                    "append" => "Add",
                    "deleteone" => "DeleteOne",
                    "getpos" => "GetPos",
                    "deleteat" => "DeleteAt",
                    "getat" => "GetAt",
                    "setat" => "SetAt",
                    "count" => "Count",
                    "add" => "Add",
                    "addat" => "AddAt",
                    "addprop" => "Add",
                    _ => char.ToUpperInvariant(propVar2.VarName[0]) + propVar2.VarName[1..]
                };
                Append(pascal);
            }
            else
            {
                node.Property.Accept(this);
            }
        }
        else
        {
            var start = _sb.Length;
            if (node.Object is LingoCallNode call)
                WriteCallExpr(call);
            else if (node.Object is LingoObjCallNode objCall)
                WriteObjCallExpr(objCall);
            else if (node.Object is LingoObjCallV4Node v4)
                WriteObjCallV4Expr(v4);
            else
                node.Object.Accept(this);
            TrimSemicolon(start);

            Append(".");
            if (node.Property is LingoVarNode propVar3)
            {
                var lower = propVar3.VarName.ToLowerInvariant();
                var pascal = lower switch
                {
                    "actorlist" => "ActorList",
                    "deleteone" => "DeleteOne",
                    "getpos" => "GetPos",
                    "deleteat" => "DeleteAt",
                    "getat" => "GetAt",
                    "setat" => "SetAt",
                    "count" => "Count",
                    "append" => "Add",
                    "add" => "Add",
                    "addat" => "AddAt",
                    "addprop" => "Add",
                    _ => char.ToUpperInvariant(propVar3.VarName[0]) + propVar3.VarName[1..]
                };
                Append(pascal);
            }
            else
            {
                node.Property.Accept(this);
            }
        }
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

    public void Visit(LingoSoundPropExprNode node)
    {
        Append("Sound(");
        node.Sound.Accept(this);
        Append(").");
        node.Property.Accept(this);
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

    public void Visit(LingoPropertyDeclStmtNode node) { }
}

