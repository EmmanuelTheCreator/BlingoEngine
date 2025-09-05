using LingoEngine.Lingo.Core;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class ClassGenerationTests
{
    private readonly LingoToCSharpConverter _converter = new();
    [Fact]
    public void BehaviorScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyBehavior",
            Source = "",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : LingoSpriteBehavior",
            "{",
            "    public MyBehaviorBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParentScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyParent",
            Source = "",
            Type = LingoScriptType.Parent
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyParentParent : LingoParentScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyParentParent(ILingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NewHandlerBecomesConstructor()
    {
        var file = new LingoScriptFile
        {
            Name = "MyParent",
            Source = string.Join('\n',
                "property myVar",
                "on new me, x",
                "  myVar = x",
                "end"),
            Type = LingoScriptType.Parent
        };
        var result = _converter.Convert(file).Trim();
        var expected = string.Join('\n',
            "using System;",
            "using LingoEngine.Lingo.Core;",
            "",
            "namespace Generated;",
            "",
            "public class MyParentParent : LingoParentScript",
            "{",
            "    public object myVar;",
            "",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyParentParent(ILingoMovieEnvironment env, GlobalVars global, object x) : base(env)",
            "    {",
            "        _global = global;",
            "        myVar = x;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MovieScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyMovie",
            Source = "",
            Type = LingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyMovieMovieScript : LingoMovieScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyMovieMovieScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BehaviorScriptWithPropertyDescriptionListImplementsInterface()
    {
        var file = new LingoScriptFile
        {
            Name = "MyBehavior",
            Source = "on getPropertyDescriptionList\nend",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList",
            "{",
            "    public MyBehaviorBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "",
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()",
            "    {",
            "        return new BehaviorPropertyDescriptionList()",
            "        ;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScriptWithPropertyDescriptionListForcesBehaviorType()
    {
        var file = new LingoScriptFile
        {
            Name = "MyScript",
            Source = "on getPropertyDescriptionList\nend",
            Type = LingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyScriptBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList",
            "{",
            "    public MyScriptBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "",
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()",
            "    {",
            "        return new BehaviorPropertyDescriptionList()",
            "        ;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ClassNameStripsLsSuffix()
    {
        var file = new LingoScriptFile
        {
            Name = "Stop_Menu_ls",
            Source = string.Empty,
            Type = LingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("class Stop_MenuBehavior", result);
    }

    [Fact]
    public void CustomSuffixesAreApplied()
    {
        var settings = new LingoToCSharpConverterSettings
        {
            BehaviorSuffix = "Beh",
            ParentSuffix = "Par",
            MovieScriptSuffix = "Mov"
        };
        var converter = new LingoToCSharpConverter(settings);

        var b = converter.ConvertClass(new LingoScriptFile { Name = "Foo_ls", Source = "", Type = LingoScriptType.Behavior });
        var p = converter.ConvertClass(new LingoScriptFile { Name = "Bar_ls", Source = "", Type = LingoScriptType.Parent });
        var m = converter.ConvertClass(new LingoScriptFile { Name = "Baz_ls", Source = "", Type = LingoScriptType.Movie });

        Assert.Contains("class FooBeh", b);
        Assert.Contains("class BarPar", p);
        Assert.Contains("class BazMov", m);
    }

    [Fact]
    public void ScriptWithFullPropertyDescriptionListCreatesBehaviorClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyComplexScript",
            Source = @"on getPropertyDescriptionList
  description = [:]
  addProp description,#myStartMembernum, [#default:0, #format:#integer, #comment:""My Start membernum:""]
  addProp description,#myEndMembernum, [#default:10, #format:#integer, #comment:""My End membernum:""]
  addProp description,#myValue, [#default:-1, #format:#integer, #comment:""My Start Value:""]
  addProp description,#mySlowDown, [#default:1, #format:#integer, #comment:""mySlowDown:""]
  addProp description,#myDataSpriteNum, [#default:1, #format:#integer, #comment:""My Sprite that contains info\n(set value to -1):""]
  addProp description,#myDataName, [#default:1, #format:#string, #comment:""Name Info:""]
  addProp description,#myWaitbeforeExecute, [#default:0, #format:#integer, #comment:""WaitTime before execute:""]
  addProp description,#myFunction, [#default:70, #format:#symbol, #comment:""function to execute:""]
  return description
end",
            Type = LingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public class MyComplexScriptBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList", result);
        Assert.Contains("public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()", result);
        Assert.Contains("return new BehaviorPropertyDescriptionList()", result);
        Assert.Contains(".Add(this, x => x.myStartMembernum, \"My Start membernum:\", 0)", result);
        Assert.Contains(".Add(this, x => x.myFunction, \"function to execute:\", \"70\")", result);
        Assert.Contains("public int myStartMembernum = 0;", result);
        Assert.Contains("public string myFunction = \"70\";", result);
    }

    [Fact]
    public void PropertyDeclarationsBecomeFields()
    {
        var file = new LingoScriptFile
        {
            Name = "MyVars",
            Source = @"property myVar1, myVar2
on startMovie
end",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public object myVar1;", result);
        Assert.Contains("public object myVar2;", result);
    }

    [Fact]
    public void PropertyTypesAreInferredFromAssignments()
    {
        var file = new LingoScriptFile
        {
            Name = "InferTypes",
            Source = @"property a, b, c, d
on startMovie
  a = 1
  b = ""hi""
  c = member(""foo"").text
  d = false
end",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public int a;", result);
        Assert.Contains("public string b;", result);
        Assert.Contains("public string c;", result);
        Assert.Contains("public bool d;", result);
    }

    [Fact]
    public void AppendInfersLingoList()
    {
        var file = new LingoScriptFile
        {
            Name = "Score",
            Source = @"property tempScore
on startMovie
  append tempScore, 5
end",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.Convert(file);
        Assert.Contains("public LingoList<int> tempScore = new();", result);
        Assert.Contains("tempScore.Add(5);", result);
    }

    [Fact]
    public void AddAtInfersLingoList()
    {
        var file = new LingoScriptFile
        {
            Name = "Numbers",
            Source = @"property nums
on startMovie
  addAt nums, 1, 10
end",
            Type = LingoScriptType.Behavior
        };
        var result = _converter.Convert(file);
        Assert.Contains("public LingoList<int> nums = new();", result);
        Assert.Contains("nums.AddAt(1, 10);", result);
    }

    [Theory]
    [InlineData("blur", "IHasBlurEvent", null)]
    [InlineData("focus", "IHasFocusEvent", null)]
    [InlineData("keyDown", "IHasKeyDownEvent", "LingoKeyEvent key")]
    [InlineData("keyUp", "IHasKeyUpEvent", "LingoKeyEvent key")]
    [InlineData("mouseWithin", "IHasMouseWithinEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseLeave", "IHasMouseLeaveEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseDown", "IHasMouseDownEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseUp", "IHasMouseUpEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseMove", "IHasMouseMoveEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseWheel", "IHasMouseWheelEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseEnter", "IHasMouseEnterEvent", "LingoMouseEvent mouse")]
    [InlineData("mouseExit", "IHasMouseExitEvent", "LingoMouseEvent mouse")]
    [InlineData("beginSprite", "IHasBeginSpriteEvent", null)]
    [InlineData("endSprite", "IHasEndSpriteEvent", null)]
    [InlineData("stepFrame", "IHasStepFrameEvent", null)]
    [InlineData("prepareFrame", "IHasPrepareFrameEvent", null)]
    [InlineData("enterFrame", "IHasEnterFrameEvent", null)]
    [InlineData("exitFrame", "IHasExitFrameEvent", null)]

    public void EventHandlersImplementInterfaces(string handler, string expectedInterface, string? param)
    {
        var file = new LingoScriptFile
        {
            Name = "MyScript",
            Source = $"on {handler} me\nend",
            Type = LingoScriptType.Behavior
        };

        var result = _converter.Convert(file);
        var classDecl = $"public class MyScriptBehavior : LingoSpriteBehavior, {expectedInterface}";
        Assert.Contains(classDecl, result);
        var methodName = char.ToUpperInvariant(handler[0]) + handler[1..];
        if (param is null)
            Assert.Contains($"public void {methodName}()", result);
        else
            Assert.Contains($"public void {methodName}({param})", result);
    }
}
