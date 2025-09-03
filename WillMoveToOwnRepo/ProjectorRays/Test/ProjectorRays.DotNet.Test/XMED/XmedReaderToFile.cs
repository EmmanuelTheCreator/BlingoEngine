using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;

namespace ProjectorRays.DotNet.Test.XMED;

public class XmedReaderToFile : IXmedReader
{
    private readonly string _sourceFile;

    public XmedReaderToFile(string sourceFile)
    {
        _sourceFile = sourceFile;
    }

    public XmedDocument Read(BufferView view)
    {
        var binData = new List<byte>();
        string path = Path.ChangeExtension(_sourceFile, ".xmed.txt");
        string path2 = Path.ChangeExtension(_sourceFile, ".xmed.bin");
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        var data = view.Data;
        int start = view.Offset;
        int end = start + view.Size;
        for (int i = start; i < end; i += 32)
        {
            int len = Math.Min(32, end - i);
            var sb = new StringBuilder(len * 3);
            for (int j = 0; j < len; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(data[i + j].ToString("X2"));
                binData.Add(data[i + j]);
            }
            writer.WriteLine(sb.ToString());
        }
        File.WriteAllBytes(path2, binData.ToArray());
        return new XmedDocument();
    }
}
