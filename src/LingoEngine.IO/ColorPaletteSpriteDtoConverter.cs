using LingoEngine.ColorPalettes;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.IO;

internal static class ColorPaletteSpriteDtoConverter
{
    public static LingoColorPaletteSpriteDTO ToDto(LingoColorPaletteSprite sprite)
    {
        return new LingoColorPaletteSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Frame = sprite.Frame,
            Member = MemberRefDtoConverter.ToDto(sprite.Member),
            Settings = sprite.GetSettings() is { } settings ? ToDto(settings) : null
        };
    }

    public static void Apply(LingoColorPaletteSpriteDTO dto, LingoColorPaletteSprite sprite)
    {
        sprite.Name = dto.Name;
        sprite.BeginFrame = dto.BeginFrame;
        sprite.EndFrame = dto.EndFrame;
        sprite.Lock = dto.Lock;
        sprite.Frame = dto.Frame;

        if (dto.Settings != null)
        {
            var settings = FromDto(dto.Settings);
            sprite.SetSettings(settings);
            if (sprite.Member != null)
            {
                sprite.Member.Cycles = settings.Cycles;
            }
        }
    }

    public static LingoColorPaletteFrameSettingsDTO ToDto(LingoColorPaletteFrameSettings settings)
    {
        return new LingoColorPaletteFrameSettingsDTO
        {
            ColorPaletteId = settings.ColorPaletteId,
            Rate = settings.Rate,
            Cycles = settings.Cycles,
            Action = (LingoColorPaletteActionDTO)settings.Action,
            TransitionOption = (LingoColorPaletteTransitionOptionDTO)settings.TransitionOption,
            CycleOption = (LingoColorPaletteCycleOptionDTO)settings.CycleOption
        };
    }

    public static LingoColorPaletteFrameSettings FromDto(LingoColorPaletteFrameSettingsDTO dto)
    {
        return new LingoColorPaletteFrameSettings
        {
            ColorPaletteId = dto.ColorPaletteId,
            Rate = dto.Rate,
            Cycles = dto.Cycles,
            Action = (LingoColorPaletteAction)dto.Action,
            TransitionOption = (LingoColorPaletteTransitionOption)dto.TransitionOption,
            CycleOption = (LingoColorPaletteCycleOption)dto.CycleOption
        };
    }
}
