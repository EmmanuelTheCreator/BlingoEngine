using LingoEngine.IO.Data.DTO;
using LingoEngine.Transitions;

namespace LingoEngine.IO;

internal static class TransitionSpriteDtoConverter
{
    public static LingoTransitionSpriteDTO ToDto(LingoTransitionSprite sprite)
    {
        return new LingoTransitionSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Member = MemberRefDtoConverter.ToDto(sprite.Member),
            Settings = sprite.GetSettings() is { } settings ? ToDto(settings) : null
        };
    }

    public static void Apply(LingoTransitionSpriteDTO dto, LingoTransitionSprite sprite)
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

    public static LingoTransitionFrameSettingsDTO ToDto(LingoTransitionFrameSettings settings)
    {
        return new LingoTransitionFrameSettingsDTO
        {
            TransitionId = settings.TransitionId,
            TransitionName = settings.TransitionName,
            Duration = settings.Duration,
            Smoothness = settings.Smoothness,
            Affects = (LingoTransitionAffectsDTO)settings.Affects,
            Rect = RectDtoConverter.ToDto(settings.Rect)
        };
    }

    public static LingoTransitionFrameSettings FromDto(LingoTransitionFrameSettingsDTO dto)
    {
        return new LingoTransitionFrameSettings
        {
            TransitionId = dto.TransitionId,
            TransitionName = dto.TransitionName,
            Duration = dto.Duration,
            Smoothness = dto.Smoothness,
            Affects = (LingoTransitionAffects)dto.Affects,
            Rect = RectDtoConverter.FromDto(dto.Rect)
        };
    }
}
