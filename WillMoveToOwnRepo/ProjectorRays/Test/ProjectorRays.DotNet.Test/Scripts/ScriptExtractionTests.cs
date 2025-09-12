using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Scripts;

public class ScriptExtractionTests
{
    private readonly ILogger<ScriptExtractionTests> _logger;

    public ScriptExtractionTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });
        _logger = factory.CreateLogger<ScriptExtractionTests>();
    }

    [Fact]
    public void DirContainsAllScriptTypes()
    {
        var path = GetDemoPath("TetriGrounds/TetriGrounds.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        dir.ParseScripts();
        dir.RestoreScriptText();

        var types = dir.Casts
            .SelectMany(c => c.Members.Values)
            .Where(m => m.Type == RaysMemberType.ScriptMember && m.Member is RaysScriptMember)
            .Select(m => ((RaysScriptMember)m.Member!).ScriptType)
            .ToList();

        Assert.Contains(RaysScriptType.ScoreScript, types);
        Assert.Contains(RaysScriptType.ParentScript, types);
        Assert.Contains(RaysScriptType.MovieScript, types);
    }

    private static string GetDemoPath(string relative)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../../../../Demo", relative);
    }
}
