using LingoEngine.Movies;

namespace LingoEngine.Transitions;

internal class LingoTransitionKeyFrameManager : LingoKeyframeManager<LingoTransitionKeyFrame,int>
{
    protected override void SetValue(LingoTransitionKeyFrame kf, int value) => kf.TransitionId = value;
    protected override LingoTransitionKeyFrame Create(int frame, int value) => new LingoTransitionKeyFrame(frame, value);

 
}
