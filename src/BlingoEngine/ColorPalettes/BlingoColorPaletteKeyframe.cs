using BlingoEngine.Animations;

namespace BlingoEngine.ColorPalettes;

public class BlingoColorPaletteKeyframe : BlingoKeyFrame<int>
{
    public BlingoColorPaletteKeyframe(int frame, int value) : base(frame, value)
    {
    }

    public int PaletteId { get; set; }

   
}

