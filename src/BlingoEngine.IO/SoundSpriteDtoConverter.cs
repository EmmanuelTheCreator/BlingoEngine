using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Sounds;

namespace BlingoEngine.IO;

internal static class SoundSpriteDtoConverter
{
    public static BlingoSpriteSoundDTO ToDto(this BlingoSpriteSound sprite)
    {
        return new BlingoSpriteSoundDTO
        {
            Name = sprite.Name,
            Channel = sprite.Channel,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Member = sprite.Sound.ToDto()
        };
    }

    public static void Apply(BlingoSpriteSoundDTO dto, BlingoSpriteSound sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;
    }
}

