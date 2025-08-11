using LingoEngine.Animations;

namespace LingoEngine.Transitions
{
    internal class LingoTransitionKeyFrame : LingoKeyFrame<int>
    {
        public int TransitionId { get; internal set; }
        public LingoTransitionKeyFrame(int frame, int value) : base(frame, value)
        {
        }

    }
}
