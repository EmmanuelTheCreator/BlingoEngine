using LingoEngine.Lingo.Core.Tokenizer;
using LingoEngine.Lingo.Core;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class LingoToCSharpConverterTests
{
    private readonly LingoToCSharpConverter _converter = new();
    [Fact]
    public void PutStatementIsConverted()
    {
        var result = _converter.Convert("put 1 into x");
        Assert.Equal("x = 1;", result.Trim());
    }

    [Fact]
    public void PutStatementWithoutIntoIsConverted()
    {
        var result = _converter.Convert("put x");
        Assert.Equal("Put(x);", result.Trim());
    }

    [Fact]
    public void AssignmentStatementIsConverted()
    {
        var result = _converter.Convert("x = 5");
        Assert.Equal("x = 5;", result.Trim());
    }

    [Fact]
    public void CallStatementIsConverted()
    {
        var result = _converter.Convert("myFunc");
        Assert.Equal("myFunc();", result.Trim());
    }

    [Fact]
    public void ReturnConstantIsConvertedToNewLine()
    {
        var result = _converter.Convert("x = \"a\" & return & \"b\"");
        Assert.Equal("x = (\"a\" + \"\\n\") + \"b\";", result.Trim());
    }

    [Fact]
    public void UnaryMinusIsHandled()
    {
        var lingo = string.Join('\n',
            "if i=99 then dirI=-dirI");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (i == 99)",
            "{",
            "    dirI = -dirI;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void MultipleReturnConstantsAreHandled()
    {
        var lingo = "x = \"a\" & return & \"b\" & return & \"c\"";
        var result = _converter.Convert(lingo);
        Assert.Equal("x = (((\"a\" + \"\\n\") + \"b\") + \"\\n\") + \"c\";", result.Trim());
    }

    [Fact]
    public void PutIntoFieldConvertsToMemberTextAssignment()
    {
        var result = _converter.Convert("put woord into field \"credits\"");
        Assert.Equal("GetMember<ILingoMemberTextBase>(\"credits\").Text = woord;", result.Trim());
    }

    [Fact]
    public void MemberLineAccessUsesTypedGetMember()
    {
        var result = _converter.Convert("member(\"MyTextMember\").line[1]");
        Assert.Equal("GetMember<ILingoMemberTextBase>(\"MyTextMember\").Line[1]", result.Trim());
    }

    [Fact]
    public void MemberWithCastNumberIsConverted()
    {
        var result = _converter.Convert("member(\"boem1\",3)");
        Assert.Equal("Member(\"boem1\", 3)", result.Trim());
    }

    [Fact]
    public void MemberLineConcatenationUsesTypedGetMember()
    {
        var lingo = @"property i,p,dirI,num
on beginsprite me
  num=me.spritenum
  woord=member(""MyTextMember"").line[1] &return& member(""MyTextMember"").line[2] &return& member(""MyTextMember"").line[3]
  
end";
        var file = new LingoScriptFile { Name = "MyScript", Source = lingo, Type = LingoScriptType.Behavior };
        var result = _converter.Convert(file);
        Assert.Contains("woord = (((GetMember<ILingoMemberTextBase>(\"MyTextMember\").Line[1] + \"\\n\") + GetMember<ILingoMemberTextBase>(\"MyTextMember\").Line[2]) + \"\\n\") + GetMember<ILingoMemberTextBase>(\"MyTextMember\").Line[3];", result.Replace("\r", ""));
    }

    [Fact]
    public void ReturnStatementWithValueIsConverted()
    {
        var result = _converter.Convert("return 5");
        Assert.Equal("return 5;", result.Trim());
    }

    [Fact]
    public void ReturnStatementWithoutValueIsOnItsOwnLine()
    {
        var lingo = "return\nput 1 into x";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "return;",
            "x = 1;");
        Assert.Equal(expected, result.Trim());
    }

    [Fact]
    public void IfStatementIsConverted()
    {
        var lingo = "if 1 then\nput 2 into x\nend if";
        var result = _converter.Convert(lingo);
        Console.WriteLine(result);
        var expected = string.Join('\n',
            "if (1)",
            "{",
            "    x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Replace("\r", "").Trim());
    }

    [Fact]
    public void RepeatWhileStatementIsConverted()
    {
        var lingo = "repeat while 1\nput 2 into x\nend repeat";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "while (1)",
            "{",
            "    x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void ExitRepeatIfStatementIsConverted()
    {
        var result = _converter.Convert("exit repeat if 1");
        Assert.Equal("if (1) break;", result.Trim());
    }

    [Fact]
    public void NextRepeatStatementIsConverted()
    {
        var result = _converter.Convert("next repeat");
        Assert.Equal("continue;", result.Trim());
    }

    [Fact]
    public void PropertyDescriptionListIsConverted()
    {
        var lingo = string.Join('\n',
            "on getPropertyDescriptionList",
            "  description = [:]",
            "  addProp description,#myMin, [#default:0, #format:#integer, #comment:\"Min Value:\"]",
            "  addProp description,#myMax, [#default:10, #format:#integer, #comment:\"Max Value:\"]",
            "  addProp description,#myValue, [#default:-1, #format:#integer, #comment:\"My Start Value:\"]",
            "  addProp description,#myStep, [#default:1, #format:#integer, #comment:\"My step:\"]",
            "  addProp description,#myDataSpriteNum, [#default:1, #format:#integer, #comment:\"My Sprite that contains info\\n(set value to -1):\"]",
            "  addProp description,#myDataName, [#default:1, #format:#string, #comment:\"Name Info:\"]",
            "  addProp description,#myWaitbeforeExecute, [#default:70, #format:#integer, #comment:\"WaitTime before execute:\"]",
            "  addProp description,#myFunction, [#default:70, #format:#symbol, #comment:\"function to execute:\"]",
            "  return description",
            "end");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()",
            "{",
            "    return new BehaviorPropertyDescriptionList()",
            "        .Add(this, x => x.myMin, \"Min Value:\", 0)",
            "        .Add(this, x => x.myMax, \"Max Value:\", 10)",
            "        .Add(this, x => x.myValue, \"My Start Value:\", -1)",
            "        .Add(this, x => x.myStep, \"My step:\", 1)",
            "        .Add(this, x => x.myDataSpriteNum, \"My Sprite that contains info\\n(set value to -1):\", 1)",
            "        .Add(this, x => x.myDataName, \"Name Info:\", \"1\")",
            "        .Add(this, x => x.myWaitbeforeExecute, \"WaitTime before execute:\", 70)",
            "        .Add(this, x => x.myFunction, \"function to execute:\", \"70\");",
            "}");
        Assert.Equal(expected.Trim(), result.Replace("\r", "").Trim());
    }

    [Fact]
    public void MemberAssignmentInIfIsConverted()
    {
        var lingo = string.Join('\n',
            "on mousedown me",
            "  if myLock=false then",
            "    myMember = member(\"Destroy1\")",
            "  end if",
            "end");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void Mousedown(LingoMouseEvent mouse)",
            "{",
            "    if (myLock == false)",
            "    {",
            "        myMember = Member(\"Destroy1\");",
            "    }",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void SendSpriteStatementIsConverted()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "B1",
                Source = "on beginSprite\r\n sendSprite 2, #doIt\r\nend\r\n",
                Type = LingoScriptType.Behavior
            },
            new LingoScriptFile
            {
                Name = "B2",
                Source = "on doIt\r\n end\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = _converter.Convert(scripts);
        var expected = string.Join('\n',
            "using System;",
            "using LingoEngine.Lingo.Core;",
            "",
            "namespace Generated;",
            "",
            "public class B1Behavior : LingoSpriteBehavior, IHasBeginSpriteEvent",
            "{",
            "    public B1Behavior(ILingoMovieEnvironment env) : base(env) { }",
            "public void BeginSprite()",
            "{",
            "    SendSprite<B2>(2, b2 => b2.doIt());",
            "}",
            "",
            "}");
        Assert.Equal(expected.Trim(), batch.ConvertedScripts["B1"].Replace("\r", "").Trim());
    }

    [Fact]
    public void SendSpriteWithArgumentIsConverted()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "B1",
                Source = "on beginSprite\r\n sendSprite 2, #doIt, 42\r\nend\r\n",
                Type = LingoScriptType.Behavior
            },
            new LingoScriptFile
            {
                Name = "B2",
                Source = "on doIt me, value\r\n end\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = _converter.Convert(scripts);
        var expected = string.Join('\n',
            "using System;",
            "using LingoEngine.Lingo.Core;",
            "",
            "namespace Generated;",
            "",
            "public class B1Behavior : LingoSpriteBehavior, IHasBeginSpriteEvent",
            "{",
            "    public B1Behavior(ILingoMovieEnvironment env) : base(env) { }",
            "public void BeginSprite()",
            "{",
            "    SendSprite<B2>(2, b2 => b2.doIt(42));",
            "}",
            "",
            "}");
        Assert.Equal(expected.Trim(), batch.ConvertedScripts["B1"].Replace("\r", "").Trim());
    }

    [Fact]
    public void SendSpriteUnknownMethodGeneratesBehavior()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "B1",
                Source = "on beginSprite\r\n sendSprite 2, #doIt\r\nend\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = _converter.Convert(scripts);
        var expected = string.Join('\n',
            "using System;",
            "using LingoEngine.Lingo.Core;",
            "",
            "namespace Generated;",
            "",
            "public class B1Behavior : LingoSpriteBehavior, IHasBeginSpriteEvent",
            "{",
            "    public B1Behavior(ILingoMovieEnvironment env) : base(env) { }",
            "public void BeginSprite()",
            "{",
            "    SendSprite<DoItBehavior>(2, doitbehavior => doitbehavior.doIt());",
            "}",
            "",
            "}");
        Assert.Equal(expected.Trim(), batch.ConvertedScripts["B1"].Replace("\r", "").Trim());
        Assert.True(batch.ConvertedScripts.ContainsKey("DoItBehavior"));
        Assert.Contains("public class DoItBehavior : LingoSpriteBehavior", batch.ConvertedScripts["DoItBehavior"]);
        Assert.Contains("public object? doIt(params object?[] args) => null;", batch.ConvertedScripts["DoItBehavior"]);
    }

    [Fact]
    public void UnknownMethodCallsMovieScript()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "M1",
                Source = "on myMovieHandler\r\n end\r\n",
                Type = LingoScriptType.Movie
            },
            new LingoScriptFile
            {
                Name = "P1",
                Source = "on beginSprite\r\n myMovieHandler\r\nend\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = _converter.Convert(scripts);
        var expected = string.Join('\n',
            "using System;",
            "using LingoEngine.Lingo.Core;",
            "",
            "namespace Generated;",
            "",
            "public class P1Behavior : LingoSpriteBehavior, IHasBeginSpriteEvent",
            "{",
            "    public P1Behavior(ILingoMovieEnvironment env) : base(env) { }",
            "public void BeginSprite()",
            "{",
            "    CallMovieScript<M1>(m1 => m1.myMovieHandler());",
            "}",
            "",
            "}");
        Assert.Equal(expected.Trim(), batch.ConvertedScripts["P1"].Replace("\r", "").Trim());
    }

    [Fact]
    public void MemberTextAccessIsConverted()
    {
        var result = _converter.Convert("member(\"T_Text\").text");
        Assert.Equal("GetMember<ILingoMemberTextBase>(\"T_Text\").Text", result.Trim());
    }

    [Fact]
    public void NewMemberCallIsConverted()
    {
        var result = _converter.Convert("_movie.newMember(#bitmap)");
        Assert.Equal("_movie.New.Bitmap()", result.Trim());
    }

    [Fact]
    public void NewMemberAssignmentIsConverted()
    {
        var result = _converter.Convert("img = _movie.newMember(#bitmap)");
        Assert.Equal("img = _movie.New.Bitmap();", result.Trim());
    }

    [Fact]
    public void VoidpAndListAreConverted()
    {
        var lingo = string.Join('\n',
            "if voidp(x) then",
            "  x = 1",
            "end if",
            "myMembers = [\"A\", \"B\"]",
            "y = myMembers[2]");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (x == null)",
            "{",
            "    x = 1;",
            "}",
            "myMembers = [\"A\", \"B\"];",
            "y = myMembers[2];");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void VoidpWithVoidLiteralIsConverted()
    {
        var lingo = string.Join('\n',
            "if voidp(void) then",
            "  x = 1",
            "end if");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (null == null)",
            "{",
            "    x = 1;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void CommentLinesArePreserved()
    {
        var lingo = string.Join('\n', "-- test", "put 1 into x");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n', "// test", "x = 1;");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void ElseIfStatementIsConverted()
    {
        var lingo = string.Join('\n',
            "if a = 1 then",
            "  x = 1",
            "else if a = 2 then",
            "  x = 2",
            "end if");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (a == 1)",
            "{",
            "    x = 1;",
            "}",
            "else if (a == 2)",
            "{",
            "    x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void SingleLineIfWithElseIsConverted()
    {
        var lingo = string.Join('\n',
            "if a = 1 then x = 1",
            "else",
            "  x = 2",
            "end if");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (a == 1)",
            "{",
            "    x = 1;",
            "}",
            "else",
            "{",
            "    x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void NestedIfInsideElseIsConverted()
    {
        var lingo = string.Join('\n',
            "if a = 1 then",
            "  x = 1",
            "else",
            "  if b = 2 then",
            "    x = 2",
            "  end if",
            "end if");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (a == 1)",
            "{",
            "    x = 1;",
            "}",
            "else if (b == 2)",
            "{",
            "    x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void MeVoidAssignmentIsIgnored()
    {
        var lingo = string.Join('\n',
            "me = void",
            "x = 5");
        var result = _converter.Convert(lingo);
        Assert.Equal("x = 5;", result.Trim());
    }

    [Fact]
    public void ReturnMeInNewIsIgnored()
    {
        var lingo = string.Join('\n',
            "on new me",
            "  return me",
            "end");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void New()",
            "{",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void StepframeAndNewHandlersAreConverted()
    {
        var lingo = @"on stepframe me
  if myDestroyAnim=true then
    if myMemberNumAnim >7 then
      myMemberNumAnim = 0
      deleteone the actorlist(me)
      me.destroy()
    end if
    myMemberNumAnim = myMemberNumAnim +1
    sprite(myNum).member = member(""Destroy""&myMemberNumAnim)
  end if
end

on new me,_Gfx,ChosenType
  if voidp(ChosenType) then ChosenType=1
  myMembers = [""Block1"",""Block2"",""Block3"",""Block4"",""Block5"",""Block6"",""Block7""]
  myMember = myMembers[ChosenType]
  myGfx = _Gfx
  myDestroyAnim = false

  return me
end";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void Stepframe()",
            "{",
            "    if (myDestroyAnim == true)",
            "    {",
            "        if (myMemberNumAnim > 7)",
            "        {",
            "            myMemberNumAnim = 0;",
            "            _movie.ActorList.DeleteOne(this);",
            "            Destroy();",
            "        }",
            "        myMemberNumAnim = myMemberNumAnim + 1;",
            "        Sprite(myNum).Member = Member(\"Destroy\" + myMemberNumAnim);",
            "    }",
            "}",
            "",
            "public void New(object _Gfx, object ChosenType)",
            "{",
            "    if (ChosenType == null)",
            "    {",
            "        ChosenType = 1;",
            "    }",
            "    myMembers = [\"Block1\", \"Block2\", \"Block3\", \"Block4\", \"Block5\", \"Block6\", \"Block7\"];",
            "    myMember = myMembers[ChosenType];",
            "    myGfx = _Gfx;",
            "    myDestroyAnim = false;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void ComparisonOperatorsAreConverted()
    {
        var lingo = string.Join('\n',
            "on cmp",
            "  if 1 < 2 then",
            "  end if",
            "  if 1 <= 2 then",
            "  end if",
            "  if 1 >= 2 then",
            "  end if",
            "  if 1 > 2 then",
            "  end if",
            "end");
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void Cmp()",
            "{",
            "    if (1 < 2)",
            "    {",
            "    }",
            "    if (1 <= 2)",
            "    {",
            "    }",
            "    if (1 >= 2)",
            "    {",
            "    }",
            "    if (1 > 2)",
            "    {",
            "    }",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact(Skip = "Normalization mismatch")]
    public void DemoNewGameScriptMatchesConvertedOutput()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", ".."));
        string ls = File.ReadAllText(Path.Combine(root, "Demo", "TetriGrounds",
            "TetriGrounds.Lingo.Original", "13_B_NewGame.ls"));
        string expected = File.ReadAllText(Path.Combine(root, "Demo",
            "TetriGrounds", "LingoEngine.Demo.TetriGrounds.Core", "Sprites",
            "Behaviors", "NewGameBehavior.cs"));

        var file = new LingoScriptFile { Name = "NewGame", Source = ls, Type = LingoScriptType.Behavior };
        string converted = _converter.Convert(file);

        static string Normalize(string s) => string.Join('\n',
            s.Split('\n', '\r')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("using ") && !l.StartsWith("namespace ")));

        Assert.Contains(Normalize(converted).ToLowerInvariant(), Normalize(expected).ToLowerInvariant());
    }

    [Fact]
    public void CaseStatementIsConverted()
    {
        var label1 = new LingoCaseLabelNode
        {
            Value = new LingoDatumNode(new LingoDatum(1)),
            Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoOne" } } }
        };
        var label2 = new LingoCaseLabelNode
        {
            Value = new LingoDatumNode(new LingoDatum(2)),
            Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoTwo" } } }
        };
        label1.NextLabel = label2;
        var caseNode = new LingoCaseStmtNode
        {
            Value = new LingoDatumNode(new LingoDatum(1)),
            FirstLabel = label1,
            Otherwise = new LingoOtherwiseNode
            {
                Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoDefault" } } }
            }
        };
        var result = CSharpWriter.Write(caseNode).Trim();
        var expected = string.Join('\n',
            "switch (1)",
            "{",
            "    case 1:",
            "        DoOne();",
            "    case 2:",
            "        DoTwo();",
            "    default:",
            "        DoDefault();",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TheExpressionIsConverted()
    {
        var node = new LingoTheExprNode { Prop = "mouseH" };
        Assert.Equal("_Mouse.MouseH", CSharpWriter.Write(node).Trim());
        node.Prop = "actorList";
        Assert.Equal("_movie.ActorList", CSharpWriter.Write(node).Trim());
        node.Prop = "banana";
        Assert.Equal("/* the banana */", CSharpWriter.Write(node).Trim());
    }

    [Fact]
    public void ObjectPropertyExpressionIsConverted()
    {
        var node = new LingoObjPropExprNode
        {
            Object = new LingoVarNode { VarName = "foo" },
            Property = new LingoVarNode { VarName = "bar" }
        };
        Assert.Equal("foo.Bar", CSharpWriter.Write(node).Trim());
    }

    [Fact]
    public void PlayAndSoundNodesAreConverted()
    {
        var play = new LingoPlayCmdStmtNode { Command = new LingoVarNode { VarName = "demo" } };
        Assert.Equal("Play(demo);", CSharpWriter.Write(play).Trim());
        var soundProp = new LingoSoundPropExprNode
        {
            Sound = new LingoVarNode { VarName = "s" },
            Property = new LingoVarNode { VarName = "p" }
        };
        Assert.Equal("Sound(s).p", CSharpWriter.Write(soundProp).Trim());
        var menuProp = new LingoMenuPropExprNode
        {
            Menu = new LingoVarNode { VarName = "m" },
            Property = new LingoVarNode { VarName = "p" }
        };
        Assert.Equal("MenuProp(m, p)", CSharpWriter.Write(menuProp).Trim());
        var del = new LingoChunkDeleteStmtNode { Chunk = new LingoVarNode { VarName = "c" } };
        Assert.Equal("DeleteChunk(c);", CSharpWriter.Write(del).Trim());
    }

    [Fact]
    public void DeclarationStatementsAreConverted()
    {
        var global = new LingoGlobalDeclStmtNode();
        global.Names.AddRange(["g1", "g2"]);
        Assert.Equal(string.Empty, CSharpWriter.Write(global).Trim());

        var prop = new LingoPropertyDeclStmtNode();
        prop.Names.AddRange(["p1", "p2"]);
        Assert.Equal(string.Empty, CSharpWriter.Write(prop).Trim());

        var inst = new LingoInstanceDeclStmtNode();
        inst.Names.AddRange(["i1"]);
        Assert.Equal(string.Empty, CSharpWriter.Write(inst).Trim());
    }

    [Fact]
    public void DeclarationStatementsAreParsedAndConverted()
    {
        var result = _converter.Convert("global g1, g2");
        Assert.Equal(string.Empty, result.Trim());

        result = _converter.Convert("property p1, p2");
        Assert.Equal(string.Empty, result.Trim());

        result = _converter.Convert("instance i1");
        Assert.Equal(string.Empty, result.Trim());
    }

    [Fact]
    public void CursorAndGoToStatementsAreConverted()
    {
        var lingo = @"on mouseUp me
  cursor -1
  go to ""Game""
end

on mouseWithin me
  cursor 280
end

on mouseleave me
  cursor -1
end";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void MouseUp(LingoMouseEvent mouse)",
            "{",
            "    Cursor = -1;",
            "    _Movie.GoTo(\"Game\");",
            "}",
            "",
            "public void MouseWithin(LingoMouseEvent mouse)",
            "{",
            "    Cursor = 280;",
            "}",
            "",
            "public void Mouseleave(LingoMouseEvent mouse)",
            "{",
            "    Cursor = -1;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void SpriteMoveAndExitFrameLoopAreConverted()
    {
        var lingo = @"on mouseDown me
  sprite(me.spriteNum).locH = sprite(me.spriteNum).locH + 5
end
on exitFrame
  go to the frame
end";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void MouseDown(LingoMouseEvent mouse)",
            "{",
            "    Sprite(SpriteNum).LocH = Sprite(SpriteNum).LocH + 5;",
            "}",
            "",
            "public void ExitFrame()",
            "{",
            "    _Movie.GoTo(_Movie.CurrentFrame);",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void NumericSpriteIndexIsConverted()
    {
        var lingo = @"on test
  sprite(263).visibility = true
end";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "public void Test()",
            "{",
            "    Sprite(263).Visibility = true;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void AccessModifierIsAppliedToHandlers()
    {
        var lingo = @"on test
end";
        var result = _converter.Convert(lingo, new ConversionOptions { MethodAccessModifier = "internal" });
        var expected = string.Join('\n',
            "internal void Test()",
            "{",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void RepeatStatementsAreConverted()
    {
        var withIn = new LingoRepeatWithInStmtNode
        {
            Variable = "v",
            List = new LingoVarNode { VarName = "lst" },
            Body = new LingoBlockNode { Children = { new LingoCallNode { Name = "Do" } } }
        };
        var expectedIn = string.Join('\n',
            "foreach (var v in lst)",
            "{",
            "    Do();",
            "}");
        Assert.Equal(expectedIn, CSharpWriter.Write(withIn).Trim());

        var withTo = new LingoRepeatWithToStmtNode
        {
            Variable = "i",
            Start = new LingoDatumNode(new LingoDatum(1)),
            End = new LingoDatumNode(new LingoDatum(3)),
            Body = new LingoBlockNode { Children = { new LingoCallNode { Name = "Step" } } }
        };
        var expectedTo = string.Join('\n',
            "for (var i = 1; i <= 3; i++)",
            "{",
            "    Step();",
            "}");
        Assert.Equal(expectedTo, CSharpWriter.Write(withTo).Trim());

        var until = new LingoRepeatUntilStmtNode(
            new LingoVarNode { VarName = "done" },
            new LingoBlockNode { Children = { new LingoCallNode { Name = "Work" } } });
        var expectedUntil = string.Join('\n',
            "do",
            "{",
            "    Work();",
            "} while (!(done));");
        Assert.Equal(expectedUntil, CSharpWriter.Write(until).Trim());

        var forever = new LingoRepeatForeverStmtNode(
            new LingoBlockNode { Children = { new LingoCallNode { Name = "Loop" } } });
        var expectedForever = string.Join('\n',
            "while (true)",
            "{",
            "    Loop();",
            "}");
        Assert.Equal(expectedForever, CSharpWriter.Write(forever).Trim());

        var times = new LingoRepeatTimesStmtNode(
            new LingoDatumNode(new LingoDatum(2)),
            new LingoBlockNode { Children = { new LingoCallNode { Name = "T" } } });
        var expectedTimes = string.Join('\n',
            "for (int i = 1; i <= 2; i++)",
            "{",
            "    T();",
            "}");
        Assert.Equal(expectedTimes, CSharpWriter.Write(times).Trim());
    }

    [Fact]
    public void ExpressionNodesAreConverted()
    {
        var within = new LingoSpriteWithinExprNode
        {
            SpriteA = new LingoVarNode { VarName = "a" },
            SpriteB = new LingoVarNode { VarName = "b" }
        };
        Assert.Equal("SpriteWithin(a, b)", CSharpWriter.Write(within).Trim());

        var last = new LingoLastStringChunkExprNode { Source = new LingoVarNode { VarName = "txt" } };
        Assert.Equal("LastChunkOf(txt)", CSharpWriter.Write(last).Trim());

        var inter = new LingoSpriteIntersectsExprNode
        {
            SpriteA = new LingoVarNode { VarName = "x" },
            SpriteB = new LingoVarNode { VarName = "y" }
        };
        Assert.Equal("SpriteIntersects(x, y)", CSharpWriter.Write(inter).Trim());

        var cnt = new LingoStringChunkCountExprNode { Source = new LingoVarNode { VarName = "str" } };
        Assert.Equal("ChunkCount(str)", CSharpWriter.Write(cnt).Trim());

        var notOp = new LingoNotOpNode { Expr = new LingoVarNode { VarName = "ok" } };
        Assert.Equal("!(ok)", CSharpWriter.Write(notOp).Trim());
    }

    [Fact]
    public void BinaryOperationIsConverted()
    {
        var inner = new LingoBinaryOpNode
        {
            Left = new LingoDatumNode(new LingoDatum(2)),
            Right = new LingoDatumNode(new LingoDatum(3)),
            Opcode = LingoBinaryOpcode.Multiply
        };
        var outer = new LingoBinaryOpNode
        {
            Left = new LingoDatumNode(new LingoDatum(1)),
            Right = inner,
            Opcode = LingoBinaryOpcode.Add
        };
        Assert.Equal("1 + (2 * 3)", CSharpWriter.Write(outer).Trim());
    }

    [Fact]
    public void ReturnAndExitStatementsAreConverted()
    {
        var retVal = new LingoReturnStmtNode(new LingoDatumNode(new LingoDatum(5)));
        Assert.Equal("return 5;", CSharpWriter.Write(retVal).Trim());
        var ret = new LingoReturnStmtNode(null);
        Assert.Equal("return;", CSharpWriter.Write(ret).Trim());
        Assert.Equal("return;", CSharpWriter.Write(new LingoExitStmtNode()).Trim());
    }

    [Fact]
    public void LoopControlStatementsAreConverted()
    {
        Assert.Equal("break;", CSharpWriter.Write(new LingoExitRepeatStmtNode()).Trim());
        var nextIf = new LingoNextRepeatIfStmtNode(new LingoDatumNode(new LingoDatum(1)));
        Assert.Equal("if (1) continue;", CSharpWriter.Write(nextIf).Trim());
    }

    [Fact]
    public void PropertyAndMiscNodesAreConverted()
    {
        var bracket = new LingoObjBracketExprNode
        {
            Object = new LingoVarNode { VarName = "arr" },
            Index = new LingoDatumNode(new LingoDatum(3))
        };
        Assert.Equal("arr[3]", CSharpWriter.Write(bracket).Trim());

        var propIndex = new LingoObjPropIndexExprNode
        {
            Object = new LingoVarNode { VarName = "o" },
            PropertyIndex = new LingoDatumNode(new LingoDatum(1))
        };
        Assert.Equal("o.prop[1]", CSharpWriter.Write(propIndex).Trim());

        var spriteProp = new LingoSpritePropExprNode
        {
            Sprite = new LingoDatumNode(new LingoDatum(2)),
            Property = new LingoVarNode { VarName = "locH" }
        };
        Assert.Equal("Sprite(2).LocH", CSharpWriter.Write(spriteProp).Trim());

        var menuItemProp = new LingoMenuItemPropExprNode
        {
            MenuItem = new LingoVarNode { VarName = "file" },
            Property = new LingoVarNode { VarName = "enabled" }
        };
        Assert.Equal("menuItem(file).enabled", CSharpWriter.Write(menuItemProp).Trim());

        var theProp = new LingoThePropExprNode { Property = new LingoVarNode { VarName = "version" } };
        Assert.Equal("TheProp(version)", CSharpWriter.Write(theProp).Trim());

        var memberExpr = new LingoMemberExprNode { Expr = new LingoVarNode { VarName = "foo" } };
        Assert.Equal("Member(foo)", CSharpWriter.Write(memberExpr).Trim());

        var soundCmd = new LingoSoundCmdStmtNode { Command = new LingoVarNode { VarName = "play" } };
        Assert.Equal("Sound(play);", CSharpWriter.Write(soundCmd).Trim());

        var hilite = new LingoChunkHiliteStmtNode { Chunk = new LingoVarNode { VarName = "c" } };
        Assert.Equal("Hilite(c);", CSharpWriter.Write(hilite).Trim());
    }

    [Fact]
    public void MemberCharRangeIsConverted()
    {
        const string src = "global scores,making\nproperty place,tempname,tempscore,win\n\non enterpress\n  addAt tempname,place,member(\"input\").char[1..5]\nend";
        var result = _converter.Convert(src);
        Assert.Contains("tempname.AddAt(place, GetMember<ILingoMemberTextBase>(\"input\").Char[1..5]);", result);
    }

    [Fact]
    public void AddPropIsConvertedToPropertyListAdd()
    {
        const string src = "global plist\n\non test\n  addProp plist,#foo,42\nend";
        var result = _converter.Convert(src);
        Assert.Contains("plist.Add(Symbol(\"foo\"), 42);", result);
    }

    [Fact]
    public void DeleteAtIsConvertedToListDeleteAt()
    {
        const string src = "global l\n\non test\n  deleteAt l,3\nend";
        var result = _converter.Convert(src);
        Assert.Contains("l.DeleteAt(3);", result);
    }

    [Fact]
    public void GetAtIsConvertedToListGetAt()
    {
        const string src = "global l\n\non test\n  x = getAt(l,2)\nend";
        var result = _converter.Convert(src);
        Assert.Contains("x = l.GetAt(2);", result);
    }

    [Fact]
    public void SetAtIsConvertedToListSetAt()
    {
        const string src = "global l\n\non test\n  setAt l,2,42\nend";
        var result = _converter.Convert(src);
        Assert.Contains("l.SetAt(2, 42);", result);
    }

    [Fact]
    public void CountIsConvertedToListCount()
    {
        const string src = "global l\n\non test\n  x = count(l)\nend";
        var result = _converter.Convert(src);
        Assert.Contains("x = l.Count;", result);
    }

    [Fact]
    public void SetPropIsConvertedToPropertyListSetProp()
    {
        const string src = "global plist\n\non test\n  setProp plist,#foo,42\nend";
        var result = _converter.Convert(src);
        Assert.Contains("plist.SetProp(Symbol(\"foo\"), 42);", result);
    }

    [Fact]
    public void DeletePropIsConvertedToPropertyListDeleteProp()
    {
        const string src = "global plist\n\non test\n  deleteProp plist,#foo\nend";
        var result = _converter.Convert(src);
        Assert.Contains("plist.DeleteProp(Symbol(\"foo\"));", result);
    }

    [Fact]
    public void DestroyHandlerActorListIsParsed()
    {
        var lingo = @"on destroy me
  if the actorlist.getpos(me) <>0 then
    _movie.actorList.deleteOne(me)
  end if
  gSpritemanager.SDestroy(myNum)
  myNum = void
  myGfx = 0
  me=void
end";
        var result = _converter.Convert(lingo);
        Assert.Contains("if (_movie.ActorList.GetPos(this) != 0)", result);
        Assert.Contains("_movie.ActorList.DeleteOne(this);", result);
    }

    [Fact]
    public void AppendActorListIsConverted()
    {
        var result = _converter.Convert("append the actorlist(me)");
        Assert.Contains("_movie.ActorList.Add(this);", result);
    }

    [Fact]
    public void AppendIsConvertedToAdd()
    {
        var result = _converter.Convert("append tempscore, 1");
        Assert.Contains("tempscore.Add(1);", result);
    }

    [Fact]
    public void SpriteMemberAssignmentIsConverted()
    {
        var result = _converter.Convert("sprite(me.spritenum).membernum = (myValue)");
        Assert.Contains("Sprite(Spritenum).Membernum = myValue;", result);
    }

    [Fact]
    public void ReverseActorListIsConverted()
    {
        var result = _converter.Convert("reverse the actorlist(me)");
        Assert.Contains("_movie.ActorList.Reverse(this);", result);
    }

    [Fact]
    public void ValueFunctionIsConverted()
    {
        var result = _converter.Convert("value(\"5\")");
        Assert.Contains("Convert.ToInt32(\"5\")", result);
    }

    [Fact]
    public void GetPosIsConverted()
    {
        var result = _converter.Convert("getPos myList, 3");
        Assert.Contains("myList.GetPos(3)", result);
    }

    [Fact]
    public void FindPosIsConverted()
    {
        var result = _converter.Convert("findPos plist, #foo");
        Assert.Contains("plist.FindPos(Symbol(\"foo\"))", result);
    }

    [Fact]
    public void GetPropAtIsConverted()
    {
        var result = _converter.Convert("getPropAt plist, 2");
        Assert.Contains("plist.GetPropAt(2)", result);
    }

    [Fact]
    public void GetaPropIsConverted()
    {
        var result = _converter.Convert("getaProp plist, #foo");
        Assert.Contains("plist.GetaProp(Symbol(\"foo\"))", result);
    }

    [Fact]
    public void SetaPropIsConverted()
    {
        var result = _converter.Convert("setaProp plist, #foo, 1");
        Assert.Contains("plist.SetaProp(Symbol(\"foo\"), 1);", result);
    }

    [Fact]
    public void MaxIsConverted()
    {
        var result = _converter.Convert("max myList");
        Assert.Contains("myList.Max()", result);
    }

    [Fact]
    public void ParentScriptIsDetected()
    {
        var lingo = "on new me\nend";
        var file = new LingoScriptFile { Name = "MyParent", Source = lingo, Type = LingoScriptType.Behavior };
        var result = _converter.Convert(file);
        Assert.Contains("class MyParentParent : LingoParentScript", result);
    }

    [Fact]
    public void MovieScriptIsDetected()
    {
        var lingo = "on StartMovie\nend";
        var file = new LingoScriptFile { Name = "MyMovie", Source = lingo, Type = LingoScriptType.Behavior };
        var result = _converter.Convert(file);
        Assert.Contains("class MyMovieMovieScript : LingoMovieScript", result);
    }

    [Fact]
    public void AlertConcatenationProducesStringParameter()
    {
        var lingo = @"on SDestroyError me,para
  alert ""SpriteDistroy received ""&&para
end";
        var file = new LingoScriptFile { Name = "Test", Source = lingo, Type = LingoScriptType.Behavior };
        var result = _converter.Convert(file);
        Assert.Contains("public void SDestroyError(string para)", result);
        Assert.Contains("alert(\"SpriteDistroy received \" + para);", result);
    }

    [Fact]
    public void CompleteScriptWithSendSpriteIsConverted()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "Counter",
                Source = @"on Beginsprite me
  if myValue=-1 then
    myValue = sendsprite(myDataSpriteNum ,#GetCounterStartData,myDataName)
    if myValue=void then
      myValue =0
    end if
    if myValue < myMin or myValue>myMax then
      myValue=0
    end if
  end if
  me.Updateme()
  myWaiter = myWaitbeforeExecute
end

on exitframe me
  if myWaiter<myWaitbeforeExecute then
    if myWaiter=myWaitbeforeExecute-1 then
      sendsprite(myDataSpriteNum ,myFunction,myDataName,myValue)
    end if
    myWaiter = myWaiter +1
  end if
  
end",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = _converter.Convert(scripts);
        var code = batch.ConvertedScripts["Counter"].Replace("\r", "");
        Assert.Contains("public class CounterBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent", code);
        Assert.Contains("public void Beginsprite()", code);
        Assert.Contains("SendSprite<GetCounterStartDataBehavior>(myDataSpriteNum", code);
        Assert.Contains("public void Exitframe()", code);
        Assert.True(batch.ConvertedScripts.ContainsKey("GetCounterStartDataBehavior"));
        var generated = batch.ConvertedScripts["GetCounterStartDataBehavior"];
        Assert.Contains("public class GetCounterStartDataBehavior : LingoSpriteBehavior", generated);
        Assert.Contains("public object? GetCounterStartData(params object?[] args) => null;", generated);
    }

    [Fact]
    public void BatchStoresMethodAndPropertyInfo()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "Example",
                Source = @"property myProp
on myHandler a,b
  a = 1
  b = member(""T"").text
end
on getPropertyDescriptionList
  description = [:]
  addProp description,#myProp,[#default:1,#format:#integer,#comment:""desc""]
  return description
end",
                Type = LingoScriptType.Behavior,
            }
};
        var batch = _converter.Convert(scripts);
        var sig = Assert.Single(batch.Methods["Example"].Where(m => m.Name.Equals("myHandler", System.StringComparison.OrdinalIgnoreCase)));
        Assert.Collection(sig.Parameters,
            p => { Assert.Equal("a", p.Name); Assert.Equal("int", p.Type); },
                    p => { Assert.Equal("b", p.Name); Assert.Equal("string", p.Type); });
        var prop = Assert.Single(batch.Properties["Example"].Where(p => p.Name == "myProp"));
        Assert.Equal("int", prop.Type);
    }

    [Fact]
    public void CastLibSaveIsConverted()
    {
        var result = _converter.Convert("castLib(\"scores\").save()");
        Assert.Equal("CastLib(\"scores\").Save();", result.Trim());
    }

    [Fact]
    public void CharToNumIsConverted()
    {
        var lingo = "if the key.charToNum = 8 then\nend if";
        var result = _converter.Convert(lingo);
        var expected = string.Join('\n',
            "if (_Key.Key.CharToNum() == 8)",
            "{",
            "}");
        Assert.Equal(expected.Trim(), result.Replace("\r", "").Trim());
    }
}
