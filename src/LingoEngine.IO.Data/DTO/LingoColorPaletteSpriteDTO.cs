namespace LingoEngine.IO.Data.DTO;

public enum LingoColorPaletteActionDTO
{
    PaletteTransition,
    ColorCycling
}

public enum LingoColorPaletteTransitionOptionDTO
{
    FadeToBlack,
    FadeToWhite,
    DontFade
}

public enum LingoColorPaletteCycleOptionDTO
{
    AutoReverse,
    Loop
}

public class LingoColorPaletteFrameSettingsDTO
{
    public int ColorPaletteId { get; set; }
    public int Rate { get; set; }
    public int Cycles { get; set; }
    public LingoColorPaletteActionDTO Action { get; set; }
    public LingoColorPaletteTransitionOptionDTO TransitionOption { get; set; }
    public LingoColorPaletteCycleOptionDTO CycleOption { get; set; }
}

public class LingoColorPaletteSpriteDTO : LingoSpriteBaseDTO
{
    public int Frame { get; set; }
    public LingoMemberRefDTO? Member { get; set; }
    public LingoColorPaletteFrameSettingsDTO? Settings { get; set; }
}
