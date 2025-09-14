using System.Collections.Generic;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.Net.RNetTerminal;

/// <summary>
/// Provides sample cast data for the console client.
/// </summary>
public static class TestCastBuilder
{
    /// <summary>
    /// Builds a dictionary of sample cast members keyed by cast name.
    /// </summary>
    public static Dictionary<string, List<LingoMemberDTO>> BuildCastData()
    {
        return new Dictionary<string, List<LingoMemberDTO>>
        {
            ["TestCast"] = new List<LingoMemberDTO>
            {
                new() { Name = "Greeting", Number = 1, NumberInCast = 1, CastLibNum = 1, Type = LingoMemberTypeDTO.Text },
                new() { Name = "Info", Number = 2, NumberInCast = 2, CastLibNum = 1, Type = LingoMemberTypeDTO.Text },
                new() { Name = "Box", Number = 3, NumberInCast = 3, CastLibNum = 1, Type = LingoMemberTypeDTO.Shape },
                new() { Name = "score", Number = 4, NumberInCast = 4, CastLibNum = 1, Type = LingoMemberTypeDTO.Text, Width = 100 },
                new() { Name = "Img30x80", Number = 5, NumberInCast = 5, CastLibNum = 1, Type = LingoMemberTypeDTO.Bitmap, Width = 30, Height = 80 }
            },
            ["ExtraCast"] = new List<LingoMemberDTO>
            {
                new() { Name = "Note", Number = 1, NumberInCast = 1, CastLibNum = 2, Type = LingoMemberTypeDTO.Text }
            }
        };
    }
}

