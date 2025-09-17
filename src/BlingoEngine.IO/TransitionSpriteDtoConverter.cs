using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Transitions;

namespace BlingoEngine.IO;

internal static class TransitionSpriteDtoConverter
{
    public static BlingoTransitionSpriteDTO ToDto(this BlingoTransitionSprite sprite)
    {
        return new BlingoTransitionSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Member = sprite.Member.ToDto(),
            Settings = sprite.GetSettings() is { } settings ? settings.ToDto() : null
        };
    }

    public static void Apply(BlingoTransitionSpriteDTO dto, BlingoTransitionSprite sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;

        if (dto.Settings != null)
        {
            sprite.SetSettings(FromDto(dto.Settings));
        }
    }

    public static BlingoTransitionFrameSettingsDTO ToDto(this BlingoTransitionFrameSettings settings)
    {
        return new BlingoTransitionFrameSettingsDTO
        {
            TransitionId = settings.TransitionId,
            TransitionName = settings.TransitionName,
            Duration = settings.Duration,
            Smoothness = settings.Smoothness,
            Affects = (BlingoTransitionAffectsDTO)settings.Affects,
            Rect = settings.Rect.ToDto()
        };
    }

    public static BlingoTransitionFrameSettings FromDto(BlingoTransitionFrameSettingsDTO dto)
    {
        return new BlingoTransitionFrameSettings
        {
            TransitionId = dto.TransitionId,
            TransitionName = dto.TransitionName,
            Duration = dto.Duration,
            Smoothness = dto.Smoothness,
            Affects = (BlingoTransitionAffects)dto.Affects,
            Rect = RectDtoConverter.FromDto(dto.Rect)
        };
    }
}

