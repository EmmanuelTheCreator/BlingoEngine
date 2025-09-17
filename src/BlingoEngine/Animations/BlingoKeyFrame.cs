using BlingoEngine.Sprites;

namespace BlingoEngine.Animations
{
    public class BlingoKeyframe : IBlingoKeyframe
    {
        public int Frame { get; set; }
        public BlingoEaseType Ease { get; set; } = BlingoEaseType.Linear;
    }
    public class BlingoKeyFrame<T> : BlingoKeyframe
    {
        public T Value { get; set; }
        

        public BlingoKeyFrame(int frame, T value)
        {
            Frame = frame;
            Value = value;
        }
    }
}

