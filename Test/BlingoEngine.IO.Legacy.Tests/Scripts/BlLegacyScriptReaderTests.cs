using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tools;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Scripts;

public class BlLegacyScriptReaderTests
{
    [Fact]
    public void ReadBehaviorDir_ExtractsBehaviorScript()
    {
        var scripts = TestContextHarness.LoadScripts("Behaviors/5spritesTest_With_Behavior.dir");

        scripts.Should().NotBeNull();
        scripts.Should().NotBeEmpty();

        var script = scripts.Should().ContainSingle(s => s.ResourceId == 5).Subject;
        script.Format.Should().Be(BlLegacyScriptFormatKind.Behavior);
        script.Bytes.Should().NotBeNull();
        script.Bytes.Should().HaveCount(148);
    }

    [Fact]
    public void ReadBehaviorDir_ExtractsBehaviorScriptContent()
    {
        var singleScripts = TestContextHarness.LoadScripts("Behaviors/5spritesTest_With_Behavior.dir");

        var singleScript = singleScripts.Should().ContainSingle(s => s.ResourceId == 5).Subject;
        singleScript.Text.Should().Be("on beginsprite\r  put 12\rend");
        singleScript.Name.Should().Be("MyTestBe");

        var multiScripts = TestContextHarness.LoadScripts("Behaviors/Two_Behaviors.dir");

        multiScripts.Should().HaveCount(2);
        multiScripts.Should().OnlyContain(s => s.Text != null && s.Name != null);

        var expectedContents = new[]
        {
            "on exitFrame me\r  put 20\r    put \"I'm in cast slot 1\"\rend",
            "on exitFrame me\r  put \"hallo\"\r  put \"I'm in cast slot 3\"\rend"
        };

        var expectedNamesByText = new Dictionary<string, string>
        {
            ["on exitFrame me\r  put 20\r    put \"I'm in cast slot 1\"\rend"] = "MyFirstBehaviorOnFrame10",
            ["on exitFrame me\r  put \"hallo\"\r  put \"I'm in cast slot 3\"\rend"] = "MeSecondBehaviorOnFrame20"
        };

        multiScripts
            .Select(script => script.Text!)
            .Should()
            .BeEquivalentTo(expectedContents);

        foreach (var script in multiScripts)
        {
            script.Name.Should().Be(expectedNamesByText[script.Text!]);
        }
    }

    [Fact(Skip ="to test")]
    public void WriteBinTxt()
    {
        var item = "Behaviors/5spritesTest_With_Behavior.dir";
        var texts = TestContextHarness.LoadScripts(item);
        var textItem = texts[0];
        var pathBin = TestContextHarness.GetAssetPath("Behaviors/" + Path.GetFileNameWithoutExtension(item) + "_" + texts[0].ResourceId + ".script.bin");
        var pathBin2 = TestContextHarness.GetAssetPath("Behaviors/" + Path.GetFileNameWithoutExtension(item) + "_" + texts[0].ResourceId + ".script.txt");
        File.WriteAllText(pathBin2, textItem.Bytes.ToHexString());
        File.WriteAllBytes(pathBin, textItem.Bytes);
        texts.Should().HaveCount(1);
        var text = texts[0];
        //var test = new BlXmedReader().Read(text.Bytes);
        //var textstring = texts[0].Bytes.ToHexString();
        //System.IO.File.WriteAllBytes(@"c:\temp\director\Text_Hallo.xmed", texts[0].Bytes);
    }
}
