using System;

namespace LingoEngine.Movies;

internal class LingoTransitionManager : LingoKeyframeManager<LingoTransitionKeyframe, int>
{
    protected override void SetValue(LingoTransitionKeyframe kf, int value) => kf.TransitionId = value;
    protected override LingoTransitionKeyframe Create(int frame, int value) => new LingoTransitionKeyframe(frame, value);
}
