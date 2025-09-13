using System.Collections.Generic;
using LingoEngine.Director.Contracts;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.Director.Client.ConsoleTest;

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
        new() { Name = "Greeting", SpriteNum = 1, MemberNum = 1, BeginFrame = 1, EndFrame = 60 },
        new() { Name = "Info", SpriteNum = 2, MemberNum = 2, BeginFrame = 1, EndFrame = 60 },
        new() { Name = "Box", SpriteNum = 3, MemberNum = 3, BeginFrame = 1, EndFrame = 60 },
        new() { Name = "Greeting", SpriteNum = 4, MemberNum = 1, BeginFrame = 1, EndFrame = 60 },
        new() { Name = "Info", SpriteNum = 5, MemberNum = 2, BeginFrame = 1, EndFrame = 60 },
    };
}
