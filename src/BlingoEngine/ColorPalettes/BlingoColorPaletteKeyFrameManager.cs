using BlingoEngine.Movies;

namespace BlingoEngine.ColorPalettes;

internal class BlingoColorPaletteKeyFrameManager : BlingoKeyframeManager<BlingoColorPaletteKeyframe, int>
{
    protected override void SetValue(BlingoColorPaletteKeyframe kf, int value) => kf.PaletteId = value;
    protected override BlingoColorPaletteKeyframe Create(int frame, int value) => new BlingoColorPaletteKeyframe(frame, value);
}

