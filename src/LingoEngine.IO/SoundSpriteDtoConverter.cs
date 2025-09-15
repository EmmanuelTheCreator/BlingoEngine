using LingoEngine.IO.Data.DTO;
using LingoEngine.Sounds;

namespace LingoEngine.IO;

internal static class SoundSpriteDtoConverter
{
    public static LingoSpriteSoundDTO ToDto(LingoSpriteSound sprite)
    {
        return new LingoSpriteSoundDTO
        {
            Name = sprite.Name,
            Channel = sprite.Channel,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Member = MemberRefDtoConverter.ToDto(sprite.Sound)
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
