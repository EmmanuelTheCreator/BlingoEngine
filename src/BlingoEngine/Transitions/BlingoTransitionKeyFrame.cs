using BlingoEngine.Animations;

namespace BlingoEngine.Transitions
{
    internal class BlingoTransitionKeyFrame : BlingoKeyFrame<int>
    {
        public int TransitionId { get; internal set; }
        public BlingoTransitionKeyFrame(int frame, int value) : base(frame, value)
        {
        }

    }
}

