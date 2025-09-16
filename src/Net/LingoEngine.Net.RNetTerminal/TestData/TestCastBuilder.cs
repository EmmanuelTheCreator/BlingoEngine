using System.Collections.Generic;
using LingoEngine.IO.Data.DTO;
using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.Net.RNetTerminal.TestData;

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
                new LingoMemberTextDTO() { Name = "Greeting", NumberInCast = 1, CastLibNum = 1, Type = LingoMemberTypeDTO.Text },
                new LingoMemberTextDTO() { Name = "Info", NumberInCast = 2, CastLibNum = 1, Type = LingoMemberTypeDTO.Text },
                new LingoMemberShapeDTO() { Name = "Box", NumberInCast = 3, CastLibNum = 1, Type = LingoMemberTypeDTO.Shape },
                new LingoMemberTextDTO() { Name = "score", NumberInCast = 4, CastLibNum = 1, Type = LingoMemberTypeDTO.Text, Width = 100 },
                new LingoMemberBitmapDTO() { Name = "Img30x80", NumberInCast = 5, CastLibNum = 1, Type = LingoMemberTypeDTO.Bitmap, Width = 30, Height = 80 }
            },
            ["ExtraCast"] = new List<LingoMemberDTO>
            {
                new LingoMemberTextDTO() { Name = "Note",NumberInCast = 1, CastLibNum = 2, Type = LingoMemberTypeDTO.Text },
                new LingoMemberTextDTO() { Name = "Note2",NumberInCast = 2, CastLibNum = 2, Type = LingoMemberTypeDTO.Text }
            }
        };
    }
}

