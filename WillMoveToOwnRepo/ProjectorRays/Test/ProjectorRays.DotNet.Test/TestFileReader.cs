using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Scores;

namespace ProjectorRays.DotNet.Test;

public static class TestFileReader
{
    public static byte[] ReadHexFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var bytes = new List<byte>();
        foreach (var line in lines)
        {
            foreach (var part in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                bytes.Add(Convert.ToByte(part, 16));
            }
        }

        return bytes.ToArray();
    }

    public static byte[] ReadScore(string dirFile)
    {
        var dump = Path.ChangeExtension(dirFile, ".score.txt");
        if (!File.Exists(dump))
        {
            var previous = RaysScoreChunk.FrameParserFactory;
            try
            {
                RaysScoreChunk.FrameParserFactory = (logger, annotator) => new RaysScoreFrameParserV2ToFile(logger, annotator, dirFile);
                var data = File.ReadAllBytes(dirFile);
                var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
                using var factory = LoggerFactory.Create(builder => { });
                var dir = new RaysDirectorFile(factory.CreateLogger("TestFileReader"), dirFile);
                dir.Read(stream);
            }
            finally
            {
                RaysScoreChunk.FrameParserFactory = previous;
            }
        }

        return ReadHexFile(dump);
    }

    public static byte[] ReadXmed(string xmedFile)
    {
        var dump = Path.ChangeExtension(xmedFile, ".xmed.txt");
        if (!File.Exists(dump))
        {
            var previous = RaysCastMemberTextRead.XmedReaderFactory;
            try
            {
                RaysCastMemberTextRead.XmedReaderFactory = () => new XmedReaderToFile(xmedFile);
                var data = File.ReadAllBytes(xmedFile);
                var view = new BufferView(data, data.Length);
                RaysCastMemberTextRead.XmedReaderFactory().Read(view);
            }
            finally
            {
                RaysCastMemberTextRead.XmedReaderFactory = previous;
            }
        }

        return ReadHexFile(dump);
    }
}
