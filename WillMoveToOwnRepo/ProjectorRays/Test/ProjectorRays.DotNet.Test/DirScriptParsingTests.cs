using System;
using Microsoft.Extensions.Logging;
using ProjectorRays.director;
using ProjectorRays.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test;

public class DirScriptParsingTests
{
    private readonly ILogger<DirScriptParsingTests> _logger;

    public DirScriptParsingTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder => builder.AddProvider(new XunitLoggerProvider(output)));
        _logger = factory.CreateLogger<DirScriptParsingTests>();
    }

    [Fact]
    public void CanParseScriptFromHex()
    {
        var data = DirectorHexData.DirData;
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, "hex.dir");
        Assert.True(dir.Read(stream));

        dir.ParseScripts();
        dir.RestoreScriptText();

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';

        string expected = "on mousedown me\n" +
                          "  sprite(me.spritenum).locH = sprite(me.spritenum).locH  +5\n" +
                          "end\n" +
                          "on exitframe\n" +
                          "  go to the frame\n" +
                          "end";

        bool found = false;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.GetScriptID() != 0)
                {
                    var text = chunk.GetScriptText().Replace("\r\n", "\n").Trim();
                    if (text == expected)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (found) break;
        }

        Assert.True(found);
    }
}
