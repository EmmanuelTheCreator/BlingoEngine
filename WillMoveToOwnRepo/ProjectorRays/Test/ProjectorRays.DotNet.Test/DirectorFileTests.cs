using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using ProjectorRays.IO;
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

    [Fact]
    public void WritesScoreBytesToFile()
    {
        var path = GetPath("Sprites/5spritesTest.dir");
        var output = Path.ChangeExtension(path, ".score.txt");
        if (File.Exists(output)) File.Delete(output);

        var previous = RaysScoreChunk.FrameParserFactory;
        try
        {
            RaysScoreChunk.FrameParserFactory = (logger, annotator) => new RaysScoreFrameParserV2ToFile(logger, annotator, path);
            var data = File.ReadAllBytes(path);
            var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger, path);
            Assert.True(dir.Read(stream));
            Assert.True(File.Exists(output));
        }
        finally
        {
            RaysScoreChunk.FrameParserFactory = previous;
        }
    }


    [Fact]
    public void ImgCastContainsBitmapMember()
    {
        var path = GetPath("Images/ImgCast.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        bool found = false;
        if (dir.Casts.Count > 0)
        {
            foreach (var id in dir.Casts[0].MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.Type == RaysMemberType.BitmapMember)
                {
                    found = true;
                    break;
                }
            }
        }
        Assert.True(found);
    }

    [Fact(Skip = "Text decoding not implemented")]
    public void TextCastTextContainsHallo()
    {
        var path = GetPath("Texts_Fields/Text_Hallo_fontsize14.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string text = string.Empty;
        if (dir.Casts.Count > 0)
        {
            foreach (var id in dir.Casts[0].MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                dir.Logger.LogInformation($"CastMember Type={chunk.Type}, Name='{chunk.GetName()}', ScriptText='{chunk.GetScriptText()}'");
                dir.Logger.LogInformation("Raw SpecificData: " + BitConverter.ToString(chunk.SpecificData.Data, chunk.SpecificData.Offset, chunk.SpecificData.Size));
                if (chunk.Type == RaysMemberType.FieldMember)
                {
                    var field = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                    if (field.DecodedText is RaysCastMemberTextRead styled)
                    {
                        text = styled.Text;
                    }
                    break;
                }
            }
        }
        Assert.Contains("Hallo", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip = "Text decoding not implemented")]
    public void DirFileTextContainsHallo()
    {
        var path = GetPath("Images/Dir_With_One_Tex_Sprite_Hallo.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string text = string.Empty;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.Type == RaysMemberType.FieldMember)
                {
                    text = chunk.GetScriptText();
                    break;
                }
            }
            if (text.Length > 0) break;
        }
        Assert.Contains("Hallo", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip = "Score data not available")]
    public void ScoreTimelineHasSpriteFrames()
    {
        var path = GetPath("Images/Dir_With_One_Img_Sprite_Hallo.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        Assert.NotNull(dir.Score);
        Assert.NotEmpty(dir.Score!.Sprites);
        var first = dir.Score.Sprites[0];
        Assert.True(first.EndFrame >= first.StartFrame);
    }

    [Fact(Skip = "Script test data not available")]
    public void CanDumpScriptText()
    {
        var path = GetPath("Texts_Fields/TextCast.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string? text = null;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                var scriptId = chunk.GetScriptID();
                if (scriptId != 0)
                {
                    var script = dir.GetScript((int)scriptId);
                    script?.Parse();
                    text = script?.ScriptText(RaysFileIO.PlatformLineEnding, false);
                    break;
                }
            }
            if (text != null) break;
        }

        Assert.NotNull(text);
    }

    [Fact(Skip = "Script test data not available")]
    public void RestoresScriptTextIntoMembers()
    {
        var path = GetPath("Texts_Fields/TextCast.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(_logger, path);
        Assert.True(dir.Read(stream));

        dir.ParseScripts();
        dir.RestoreScriptText();

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        bool found = false;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.GetScriptID() != 0 && !string.IsNullOrEmpty(chunk.GetScriptText()))
                {
                    found = true;
                    break;
                }
            }
            if (found) break;
        }

        Assert.True(found);
    }

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
