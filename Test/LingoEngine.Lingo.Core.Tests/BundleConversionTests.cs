using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using LingoEngine.Lingo.Core;

namespace LingoEngine.Lingo.Core.Tests;

public class BundleConversionTests
{
    private readonly LingoToCSharpConverter _converter = new();

    [Fact]
    public void BundleConversionTranslatesSimpleScript()
    {
        var script = new LingoScriptFile("Simple", "on startMovie\nput 1 into x\nend");
        _converter.Convert(new[] { script });
        Assert.Contains("x = 1;", script.CSharp);
    }

    [Fact]
    public void DemoScriptsAreConvertedToValidClasses()
    {
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Demo", "TetriGrounds", "TetriGrounds.Lingo.Original"));
        var files = Directory.GetFiles(baseDir, "*.ls");
        var scripts = files
            .Select(f => new LingoScriptFile(Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)))
            .ToList();
        _converter.Convert(scripts);
        var successful = scripts.Where(s => string.IsNullOrEmpty(s.Errors)).ToList();
        Assert.NotEmpty(successful);
        Assert.All(successful, s =>
        {
            var match = Regex.Match(s.CSharp, @"class\s+(\w+)");
            Assert.True(match.Success);
            Assert.True(char.IsLetter(match.Groups[1].Value[0]));
            Assert.DoesNotMatch(@"^L\d", match.Groups[1].Value);
        });
    }

    [Fact]
    public void DemoScriptsAreWrittenToGeneratedFolder()
    {
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Demo", "TetriGrounds", "TetriGrounds.Lingo.Original"));
        var generatedDir = Path.Combine(baseDir, "Generated");
        if (Directory.Exists(generatedDir))
            Directory.Delete(generatedDir, true);
        Directory.CreateDirectory(generatedDir);

        var files = Directory.GetFiles(baseDir, "*.ls");
        var scripts = files
            .Select(f => new LingoScriptFile(Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)))
            .ToList();
        _converter.Convert(scripts);

        foreach (var script in scripts)
        {
            var safeName = Regex.Replace(script.Name, @"[^A-Za-z0-9_]+", "_");
            File.WriteAllText(Path.Combine(generatedDir, $"{safeName}.cs"), script.CSharp ?? string.Empty);
        }

        Assert.Equal(scripts.Count, Directory.GetFiles(generatedDir, "*.cs").Length);
    }

    [Fact]
    public void SpriteManagerScriptIsConverted()
    {
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Demo", "TetriGrounds", "TetriGrounds.Lingo.Original"));
        var path = Path.Combine(baseDir, "3_SpriteManager.ls");
        var script = new LingoScriptFile("3_SpriteManager", File.ReadAllText(path));
        _converter.Convert(new[] { script });
        Assert.True(string.IsNullOrEmpty(script.Errors));
        Assert.False(string.IsNullOrWhiteSpace(script.CSharp));
    }

    [Fact]
    public void PropertyTypesAreInferredFromAssignments()
    {
        var source = @"property myMember, myMembers, myMemberNumAnim, myDestroyAnim, myColor\n" +
                     "on getPropertyDescriptionList\n" +
                     "  addProp description,#myColor,[#default:rgb(0),#format:#color]\n" +
                     "  return description\n" +
                     "end\n" +
                     "on startMovie\n" +
                     "  myMembers = [\"Block1\", \"Block2\"]\n" +
                     "  myMember = myMembers[1]\n" +
                     "  myMemberNumAnim = 0\n" +
                     "  myDestroyAnim = true\n" +
                     "end";
        var script = new LingoScriptFile("10_Block", source, ScriptDetectionType.Behavior);
        _converter.Convert(new[] { script });
        Assert.Contains("class BlockBehavior", script.CSharp);
        Assert.Contains("public string myMember;", script.CSharp);
        Assert.Contains("public LingoList<string> myMembers = new();", script.CSharp);
        Assert.Contains("public int myMemberNumAnim;", script.CSharp);
        Assert.Contains("public bool myDestroyAnim;", script.CSharp);
        Assert.Contains("public AColor myColor = AColor.FromCode(0);", script.CSharp);
    }

    [Fact]
    public void ConstructorParameterTypesAreInferred()
    {
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Demo", "TetriGrounds", "TetriGrounds.Lingo.Original"));
        var path = Path.Combine(baseDir, "3_SpriteManager.ls");
        var script = new LingoScriptFile("3_SpriteManager", File.ReadAllText(path), ScriptDetectionType.Parent);
        _converter.Convert(new[] { script });
        Assert.Contains("public int pNum;", script.CSharp);
        Assert.Contains("SpriteManagerParentScript(ILingoMovieEnvironment env, GlobalVars global, int _beginningsprite)", script.CSharp, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ScriptNewCallInstantiatesWithEnvAndGlobals()
    {
        var spriteMgr = new LingoScriptFile("3_SpriteManager", "on startMovie\nend", ScriptDetectionType.Parent);
        var bgSource = string.Join('\n',
            "on startMovie",
            "  gSpriteManager = script(\"SpriteManager\")",
            "  .new(100)",
            "end");
        var bg = new LingoScriptFile("2_Bg_Script", bgSource);
        _converter.Convert(new[] { spriteMgr, bg });
        Assert.Contains("gSpriteManager = new SpriteManagerParentScript(_env, _globalvars, 100)", bg.CSharp);
    }
}
