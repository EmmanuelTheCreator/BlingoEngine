using System.Collections.Generic;
using LingoEngine.Net.RNetContracts;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.Net.RNetTerminal;

/// <summary>
/// Supplies sample movie and sprite data for the console client.
/// </summary>
public static class TestMovieBuilder
{
    /// <summary>
    /// Builds a sample movie state.
    /// </summary>
    public static MovieStateDto BuildMovieState() => new MovieStateDto(0, 60, false);

    /// <summary>
    /// Provides sample sprite channel data.
    /// </summary>
    public static IReadOnlyList<Lingo2DSpriteDTO> BuildSprites() => new List<Lingo2DSpriteDTO>
    {
        new() { Name = "Greeting", SpriteNum = 1, CastLibNum = 1, MemberNum = 1, BeginFrame = 1, EndFrame = 60, LocH = 100, LocV = 100 },
        new() { Name = "Info", SpriteNum = 2, CastLibNum = 1, MemberNum = 2, BeginFrame = 1, EndFrame = 60, LocH = 300, LocV = 200 },
        new() { Name = "Box", SpriteNum = 3, CastLibNum = 1, MemberNum = 3, BeginFrame = 1, EndFrame = 60, LocH = 400, LocV = 300 },
        new() { Name = "Greeting", SpriteNum = 4, CastLibNum = 1, MemberNum = 1, BeginFrame = 1, EndFrame = 60, LocH = 150, LocV = 400 },
        new() { Name = "Info", SpriteNum = 5, CastLibNum = 1, MemberNum = 2, BeginFrame = 1, EndFrame = 60, LocH = 500, LocV = 120 },
        new() { Name = "score", SpriteNum = 6, CastLibNum = 1, MemberNum = 4, BeginFrame = 1, EndFrame = 60, LocH = 50, LocV = 50, Width = 100, Height = 20 },
        new() { Name = "Img30x80", SpriteNum = 7, CastLibNum = 1, MemberNum = 5, BeginFrame = 1, EndFrame = 60, LocH = 250, LocV = 350, Width = 30, Height = 80 },
    };
}
