using LingoEngine.Movies;

namespace LingoEngine.Tempos;

internal class LingoTempoKeyFrameManager : LingoKeyframeManager<LingoTempoKeyframe, int>
{
    protected override void SetValue(LingoTempoKeyframe kf, int value) => kf.Fps = value;
    protected override LingoTempoKeyframe Create(int frame, int value) => new LingoTempoKeyframe(frame, value);
}
