using LingoEngine.IO.Data.DTO;
using LingoEngine.Sounds;

namespace LingoEngine.IO;

internal static class SoundSpriteDtoConverter
{
    public static LingoSpriteSoundDTO ToDto(this LingoSpriteSound sprite)
    {
        return new LingoSpriteSoundDTO
        {
            Name = sprite.Name,
            Channel = sprite.Channel,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Member = sprite.Sound.ToDto()
        };
    }

    public static void Apply(LingoSpriteSoundDTO dto, LingoSpriteSound sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;
    }
}
