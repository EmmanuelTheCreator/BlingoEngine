using System.Collections.Generic;
using LingoEngine.Director.Contracts;

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
    public static IReadOnlyList<SpriteInfo> BuildSprites() => new List<SpriteInfo>
    {
        new SpriteInfo(1, 1, 60, 1, "Greeting"),
        new SpriteInfo(2, 1, 60, 2, "Info"),
        new SpriteInfo(3, 1, 60, 3, "Box"),
        new SpriteInfo(4, 1, 60, 4, "Greeting"),
        new SpriteInfo(5, 1, 60, 5, "Info"),
    };

    /// <summary>
    /// Describes a sprite block inside the score.
    /// </summary>
    public sealed record SpriteInfo(int Channel, int Start, int End, int Number, string MemberName);
}
