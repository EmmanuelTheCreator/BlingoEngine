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
            "public class MyParentParentScript : LingoParentScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyParentParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
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
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()",
            "    {",
            "    };",
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
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()",
            "    {",
            "    };",
            "}");
        Assert.Equal(expected, result);
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
        Assert.Contains("public BehaviorPropertyDescriptionList? GetPropertyDescriptionList() => new()", result);
        Assert.Contains("{ this, x => x.myStartMembernum, \"My Start membernum:\", 0 }", result);
        Assert.Contains("{ this, x => x.myFunction, \"function to execute:\", \"70\" }", result);
    }
}
