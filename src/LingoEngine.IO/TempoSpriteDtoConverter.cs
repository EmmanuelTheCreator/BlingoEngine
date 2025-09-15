using LingoEngine.IO.Data.DTO;
using LingoEngine.Tempos;

namespace LingoEngine.IO;

internal static class TempoSpriteDtoConverter
{
    public static LingoTempoSpriteDTO ToDto(this LingoTempoSprite sprite)
    {
        return new LingoTempoSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Frame = sprite.Frame,
            Action = (LingoTempoSpriteActionDTO)sprite.Action,
            Tempo = sprite.Tempo,
            WaitSeconds = sprite.WaitSeconds,
            CueChannel = sprite.CueChannel,
            CuePoint = sprite.CuePoint
        };
    }

    public static void Apply(LingoTempoSpriteDTO dto, LingoTempoSprite sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;
        sprite.Frame = dto.Frame;
        sprite.Action = (LingoTempoSpriteAction)dto.Action;
        sprite.Tempo = dto.Tempo;
        sprite.WaitSeconds = dto.WaitSeconds;
        sprite.CueChannel = dto.CueChannel;
        sprite.CuePoint = dto.CuePoint;
    }
}
