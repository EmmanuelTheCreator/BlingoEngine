using System.Collections.Generic;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.Net.RNetTerminal.TestData;

/// <summary>
/// Provides sample cast data for the console client.
/// </summary>
public static class TestCastBuilder
{
    /// <summary>
    /// Builds a dictionary of sample cast members keyed by cast name.
    /// </summary>
    public static Dictionary<string, List<BlingoMemberDTO>> BuildCastData()
    {
        return new Dictionary<string, List<BlingoMemberDTO>>
        {
            ["TestCast"] = new List<BlingoMemberDTO>
            {
                new BlingoMemberTextDTO() { Name = "Greeting", NumberInCast = 1, CastLibNum = 1, Type = BlingoMemberTypeDTO.Text },
                new BlingoMemberTextDTO() { Name = "Info", NumberInCast = 2, CastLibNum = 1, Type = BlingoMemberTypeDTO.Text },
                new BlingoMemberShapeDTO() { Name = "Box", NumberInCast = 3, CastLibNum = 1, Type = BlingoMemberTypeDTO.Shape },
                new BlingoMemberTextDTO() { Name = "score", NumberInCast = 4, CastLibNum = 1, Type = BlingoMemberTypeDTO.Text, Width = 100 },
                new BlingoMemberBitmapDTO() { Name = "Img30x80", NumberInCast = 5, CastLibNum = 1, Type = BlingoMemberTypeDTO.Bitmap, Width = 30, Height = 80 }
            },
            ["ExtraCast"] = new List<BlingoMemberDTO>
            {
                new BlingoMemberTextDTO() { Name = "Note",NumberInCast = 1, CastLibNum = 2, Type = BlingoMemberTypeDTO.Text },
                new BlingoMemberTextDTO() { Name = "Note2",NumberInCast = 2, CastLibNum = 2, Type = BlingoMemberTypeDTO.Text }
            }
        };
    }
}


