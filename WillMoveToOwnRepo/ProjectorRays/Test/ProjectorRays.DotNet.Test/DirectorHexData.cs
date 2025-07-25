using System;
using System.Linq;

namespace ProjectorRays.DotNet.Test;

public static class DirectorHexData
{
    private const string HexString = @"58 46 49 52 2a 3a 00 00 33 39 56 4d 70 61 6d 69 18 00 00 00 01 00 00 00 2c 00 00 00 44 07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 70 61 6d 6d 38 03 00 00 18 00 14 00 28 00 00 00 23 00 00 00 1c 00 00 00 ff ff ff ff 0f 00 00 00 58 46 49 52 2a 3a 00 00 00 00 00 00 01 00 00 00 00 00 00 00";

    public static byte[] DirData => HexString
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .Select(h => Convert.ToByte(h, 16))
        .ToArray();
}
