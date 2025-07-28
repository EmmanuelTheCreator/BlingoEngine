namespace LingoEngine.Movies;

public class LingoColorPaletteKeyframe : ILingoKeyframe
{
    public int Frame { get; set; }
    public int PaletteId { get; set; }

    public LingoColorPaletteKeyframe(int frame, int paletteId)
    {
        Frame = frame;
        PaletteId = paletteId;
    }
}
