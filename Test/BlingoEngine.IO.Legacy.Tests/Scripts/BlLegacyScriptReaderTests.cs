using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tools;
using FluentAssertions;
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

    [Fact]
    public void ReadSyntheticScripts_IdentifiesBehaviorMovieAndParentCategories()
    {
        var expectedOrder = new[]
        {
            BlLegacyScriptFormatKind.Behavior,
            BlLegacyScriptFormatKind.Movie,
            BlLegacyScriptFormatKind.Parent
        };

        using var stream = new MemoryStream();
        using var context = new ReaderContext(stream, "synthetic.dir", leaveOpen: true);
        context.RegisterRifxOffset(0);

        var dataBlock = new BlDataBlock();
        dataBlock.Format.ArchiveVersion = 0;
        context.RegisterDataBlock(dataBlock);

        var castTag = BlTag.Register("CASt");
        var scriptTag = BlTag.Register("Lscr");

        var expectedScripts = new List<(int ScriptId, BlLegacyScriptFormatKind Format)>();
        var nextResourceId = 1;

        foreach (var format in expectedOrder)
        {
            var scriptId = nextResourceId++;
            var castId = nextResourceId++;

            var castPayload = BuildCastMemberPayload(scriptId, format);
            var castOffset = WriteChunk(stream, "CASt", castPayload);
            context.AddResource(new BlLegacyResourceEntry(castId, castTag, (uint)castPayload.Length, castOffset, 0, 0, 0));

            var scriptPayload = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var scriptOffset = WriteChunk(stream, "Lscr", scriptPayload);
            context.AddResource(new BlLegacyResourceEntry(scriptId, scriptTag, (uint)scriptPayload.Length, scriptOffset, 0, 0, 0));

            expectedScripts.Add((scriptId, format));
        }

        stream.Position = 0;
        context.Reader.Position = 0;

        var reader = new BlLegacyScriptReader(context);
        var scripts = reader.Read();

        scripts.Should().HaveCount(expectedScripts.Count);

        for (var i = 0; i < expectedScripts.Count; i++)
        {
            var expected = expectedScripts[i];
            var script = scripts[i];

            script.ResourceId.Should().Be(expected.ScriptId);
            script.Format.Should().Be(expected.Format);
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

    private static byte[] BuildCastMemberPayload(int scriptResourceId, BlLegacyScriptFormatKind format)
    {
        const int infoLength = 0x80;
        var infoData = new byte[infoLength];
        BinaryPrimitives.WriteInt32BigEndian(infoData.AsSpan(8, 4), scriptResourceId);
        BinaryPrimitives.WriteInt32BigEndian(infoData.AsSpan(16, 4), scriptResourceId);

        var selector = GetSelectorByte(format);
        var specificData = new byte[] { 0x00, selector };

        var payload = new byte[12 + infoLength + specificData.Length];
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(0, 4), 11);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)infoLength);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), (uint)specificData.Length);

        infoData.CopyTo(payload.AsSpan(12, infoLength));
        specificData.CopyTo(payload.AsSpan(12 + infoLength));

        return payload;
    }

    private static uint WriteChunk(Stream stream, string tag, byte[] payload)
    {
        stream.Position = stream.Length;
        var offset = (uint)stream.Position;

        var tagBytes = Encoding.ASCII.GetBytes(tag);
        stream.Write(tagBytes, 0, tagBytes.Length);

        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(lengthBytes, (uint)payload.Length);
        stream.Write(lengthBytes);

        stream.Write(payload, 0, payload.Length);
        return offset;
    }

    private static byte GetSelectorByte(BlLegacyScriptFormatKind format)
    {
        return format switch
        {
            BlLegacyScriptFormatKind.Behavior => 0x01,
            BlLegacyScriptFormatKind.Movie => 0x03,
            BlLegacyScriptFormatKind.Parent => 0x07,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported script format for synthetic CASt payload.")
        };
    }
}
