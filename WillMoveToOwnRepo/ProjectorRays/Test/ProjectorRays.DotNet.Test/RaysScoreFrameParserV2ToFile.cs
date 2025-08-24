using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores;
using ProjectorRays.director.Scores.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectorRays.DotNet.Test;

public class RaysScoreFrameParserV2ToFile : IRaysScoreFrameParserV2
{
    private readonly ILogger _logger;
    private readonly RayStreamAnnotatorDecorator _annotator;
    private readonly string _sourceFile;

    public RaysScoreFrameParserV2ToFile(ILogger logger, RayStreamAnnotatorDecorator annotator, string sourceFile)
    {
        _logger = logger;
        _annotator = annotator;
        _sourceFile = sourceFile;
    }

    public List<RaySprite> ParseScore(ReadStream stream)
    {
        var data = stream.ReadBytes(stream.BytesLeft);
        WriteHex(data);
        return new();
    }

    private void WriteHex(byte[] data)
    {
        string path = Path.ChangeExtension(_sourceFile, ".score.txt");
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        for (int i = 0; i < data.Length; i += 32)
        {
            int len = Math.Min(32, data.Length - i);
            var sb = new StringBuilder(len * 3);
            for (int j = 0; j < len; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(data[i + j].ToString("X2"));
            }
            writer.WriteLine(sb.ToString());
        }
    }
}

