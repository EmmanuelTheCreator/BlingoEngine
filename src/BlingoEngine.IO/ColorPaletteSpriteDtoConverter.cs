using BlingoEngine.ColorPalettes;
using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.IO;

internal static class ColorPaletteSpriteDtoConverter
{
    public static BlingoColorPaletteSpriteDTO ToDto(this BlingoColorPaletteSprite sprite)
    {
        return new BlingoColorPaletteSpriteDTO
        {
            Name = sprite.Name,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.Lock,
            Frame = sprite.Frame,
            Member = sprite.Member.ToDto(),
            Settings = sprite.GetSettings() is { } settings ? settings.ToDto() : null
        };
    }

    public static void Apply(BlingoColorPaletteSpriteDTO dto, BlingoColorPaletteSprite sprite)
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

    public static BlingoColorPaletteFrameSettingsDTO ToDto(this BlingoColorPaletteFrameSettings settings)
    {
        return new BlingoColorPaletteFrameSettingsDTO
        {
            ColorPaletteId = settings.ColorPaletteId,
            Rate = settings.Rate,
            Cycles = settings.Cycles,
            Action = (BlingoColorPaletteActionDTO)settings.Action,
            TransitionOption = (BlingoColorPaletteTransitionOptionDTO)settings.TransitionOption,
            CycleOption = (BlingoColorPaletteCycleOptionDTO)settings.CycleOption
        };
    }

    public static BlingoColorPaletteFrameSettings FromDto(BlingoColorPaletteFrameSettingsDTO dto)
    {
        return new BlingoColorPaletteFrameSettings
        {
            ColorPaletteId = dto.ColorPaletteId,
            Rate = dto.Rate,
            Cycles = dto.Cycles,
            Action = (BlingoColorPaletteAction)dto.Action,
            TransitionOption = (BlingoColorPaletteTransitionOption)dto.TransitionOption,
            CycleOption = (BlingoColorPaletteCycleOption)dto.CycleOption
        };
    }
}

