using BlingoEngine.Movies;

namespace BlingoEngine.Transitions;

internal class BlingoTransitionKeyFrameManager : BlingoKeyframeManager<BlingoTransitionKeyFrame,int>
{
    protected override void SetValue(BlingoTransitionKeyFrame kf, int value) => kf.TransitionId = value;
    protected override BlingoTransitionKeyFrame Create(int frame, int value) => new BlingoTransitionKeyFrame(frame, value);

 
}

