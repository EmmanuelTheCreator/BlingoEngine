using System.Linq;
using FluentAssertions;
using Xunit;
using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Tests.Helpers;

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
}
