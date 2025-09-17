using BlingoEngine.Members;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Sprites;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>Helpers for converting engine objects to RNet DTOs.</summary>
internal static class DtoExtensions
{
    /// <summary>Converts a sprite to its network DTO.</summary>
    public static RNetSpriteDto ToDto(this BlingoSprite2D sprite)
    {
        int castLib = 0;
        int memberNum = 0;
        if (sprite.Member is IBlingoMember m)
        {
            castLib = m.CastLibNum;
            memberNum = m.NumberInCast;
        }

        return new RNetSpriteDto(
            sprite.SpriteNum,
            sprite.BeginFrame,
            sprite.LocZ,
            castLib,
            memberNum,
            (int)sprite.LocH,
            (int)sprite.LocV,
            (int)sprite.Width,
            (int)sprite.Height,
            (int)sprite.Rotation,
            (int)sprite.Skew,
            (int)sprite.Blend,
            sprite.Ink);
    }
}

