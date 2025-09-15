using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost;

/// <summary>Helpers for converting engine objects to RNet DTOs.</summary>
internal static class DtoExtensions
{
    /// <summary>Converts a sprite to its network DTO.</summary>
    public static RNetSpriteDto ToDto(this LingoSprite2D sprite)
    {
        var memberId = sprite.Member is ILingoMember m ? (m.CastLibNum * 131114) + m.NumberInCast : 0;
        return new RNetSpriteDto(sprite.SpriteNum, sprite.LocZ, memberId, (int)sprite.LocH, (int)sprite.LocV, (int)sprite.Width,
            (int)sprite.Height, (int)sprite.Rotation, (int)sprite.Skew, (int)sprite.Blend, sprite.Ink);
    }
}
