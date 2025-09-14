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
    public static IReadOnlyList<LingoSpriteDTO> BuildSprites() => new List<LingoSpriteDTO>
    {
        new() { Name = "Greeting", SpriteNum = 1, MemberNum = 1, BeginFrame = 1, EndFrame = 60, LocH = 100, LocV = 100 },
        new() { Name = "Info", SpriteNum = 2, MemberNum = 2, BeginFrame = 1, EndFrame = 60, LocH = 300, LocV = 200 },
        new() { Name = "Box", SpriteNum = 3, MemberNum = 3, BeginFrame = 1, EndFrame = 60, LocH = 400, LocV = 300 },
        new() { Name = "Greeting", SpriteNum = 4, MemberNum = 1, BeginFrame = 1, EndFrame = 60, LocH = 150, LocV = 400 },
        new() { Name = "Info", SpriteNum = 5, MemberNum = 2, BeginFrame = 1, EndFrame = 60, LocH = 500, LocV = 120 },
    };
}
