using System;

namespace LingoEngine.Movies;

internal class LingoTempoManager : LingoKeyframeManager<LingoTempoKeyframe, int>
{
    protected override void SetValue(LingoTempoKeyframe kf, int value) => kf.Fps = value;
    protected override LingoTempoKeyframe Create(int frame, int value) => new LingoTempoKeyframe(frame, value);
}
