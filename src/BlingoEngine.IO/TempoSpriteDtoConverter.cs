using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Tempos;

namespace BlingoEngine.IO;

internal static class TempoSpriteDtoConverter
{
    public static BlingoTempoSpriteDTO ToDto(this BlingoTempoSprite sprite)
    {
        return new BlingoTempoSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Action = (BlingoTempoSpriteActionDTO)sprite.Action,
            Tempo = sprite.Tempo,
            WaitSeconds = sprite.WaitSeconds,
            CueChannel = sprite.CueChannel,
            CuePoint = sprite.CuePoint
        };
    }

    public static void Apply(BlingoTempoSpriteDTO dto, BlingoTempoSprite sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;
        sprite.Action = (BlingoTempoSpriteAction)dto.Action;
        sprite.Tempo = dto.Tempo;
        sprite.WaitSeconds = dto.WaitSeconds;
        sprite.CueChannel = dto.CueChannel;
        sprite.CuePoint = dto.CuePoint;
    }
}

