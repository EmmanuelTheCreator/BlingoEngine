using BlingoEngine.Movies;

namespace BlingoEngine.Tempos;

internal class BlingoTempoKeyFrameManager : BlingoKeyframeManager<BlingoTempoKeyframe, int>
{
    protected override void SetValue(BlingoTempoKeyframe kf, int value) => kf.Fps = value;
    protected override BlingoTempoKeyframe Create(int frame, int value) => new BlingoTempoKeyframe(frame, value);
}

