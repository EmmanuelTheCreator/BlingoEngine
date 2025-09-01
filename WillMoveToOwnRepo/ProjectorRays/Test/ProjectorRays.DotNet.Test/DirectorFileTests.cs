using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test;

public class DirectorFileTests
{
    private readonly ILogger<DirectorFileTests> _logger;

    public DirectorFileTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<DirectorFileTests>();
    }

    [Theory]
    //[InlineData("Dir_With_One_Img_Sprite_Hallo.dir")]
    [InlineData("Sprites/5spritesTest.dir")]
    //[InlineData("KeyFramesTestMultiple.dir")]
    //[InlineData("KeyFrames_Lenear5.dir")]
    //[InlineData("KeyFrames_Lenear1234_deleteFrame3.dir")]
    //[InlineData("Animation_types.dir")]
    //[InlineData("KeyFramesTest.dir")]
    //[InlineData("SpriteLock.dir")]
    //[InlineData("5spritesTest_With_Behavior.dir")]
    //[InlineData("Dir_With_One_Tex_Sprite_Hallo.dir")]
    public void CanReadDirectorFile(string fileName)
    {
        var path = GetPath(fileName);
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, fileName);
        Assert.True(dir.Read(stream));
    }


    

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
