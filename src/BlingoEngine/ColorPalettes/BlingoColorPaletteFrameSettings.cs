namespace BlingoEngine.ColorPalettes;

public class BlingoColorPaletteFrameSettings
{
    public int ColorPaletteId { get; set; }
    /// <summary>
    /// Range between 1 to 30 FPS
    /// in FPS
    /// </summary>
    public int Rate { get; set; }
    /// <summary>
    /// When Action is Cycling, the number of cycles.
    /// </summary>
    public int Cycles { get; set; } = 10;
    public BlingoColorPaletteAction Action { get; set; } = BlingoColorPaletteAction.PaletteTransition;
    public BlingoColorPaletteTransitionOption TransitionOption { get; set; } = BlingoColorPaletteTransitionOption.DontFade;
    public BlingoColorPaletteCycleOption CycleOption { get; set; } = BlingoColorPaletteCycleOption.AutoReverse;
}
