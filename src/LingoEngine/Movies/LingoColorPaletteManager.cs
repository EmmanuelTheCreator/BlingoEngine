using System;

namespace LingoEngine.Movies;

internal class LingoColorPaletteManager : LingoKeyframeManager<LingoColorPaletteKeyframe, int>
{
    protected override void SetValue(LingoColorPaletteKeyframe kf, int value) => kf.PaletteId = value;
    protected override LingoColorPaletteKeyframe Create(int frame, int value) => new LingoColorPaletteKeyframe(frame, value);
}
