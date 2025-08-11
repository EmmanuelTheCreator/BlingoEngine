using LingoEngine.Animations;

namespace LingoEngine.ColorPalettes;

public class LingoColorPaletteKeyframe : LingoKeyFrame<int>
{
    public LingoColorPaletteKeyframe(int frame, int value) : base(frame, value)
    {
    }

    public int PaletteId { get; set; }

   
}
