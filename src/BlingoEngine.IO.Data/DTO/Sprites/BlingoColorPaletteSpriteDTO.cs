using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites;

public enum BlingoColorPaletteActionDTO
{
    PaletteTransition,
    ColorCycling
}

public enum BlingoColorPaletteTransitionOptionDTO
{
    FadeToBlack,
    FadeToWhite,
    DontFade
}

public enum BlingoColorPaletteCycleOptionDTO
{
    AutoReverse,
    Loop
}

public class BlingoColorPaletteFrameSettingsDTO
{
    public int ColorPaletteId { get; set; }
    public int Rate { get; set; }
    public int Cycles { get; set; }
    public BlingoColorPaletteActionDTO Action { get; set; }
    public BlingoColorPaletteTransitionOptionDTO TransitionOption { get; set; }
    public BlingoColorPaletteCycleOptionDTO CycleOption { get; set; }
}

public class BlingoColorPaletteSpriteDTO : BlingoSpriteBaseDTO
{
    public int Frame { get; set; }
    public BlingoMemberRefDTO? Member { get; set; }
    public BlingoColorPaletteFrameSettingsDTO? Settings { get; set; }
}

