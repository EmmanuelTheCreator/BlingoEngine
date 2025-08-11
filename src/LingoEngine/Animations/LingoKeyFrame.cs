using LingoEngine.Sprites;

namespace LingoEngine.Animations
{
    public class LingoKeyframe : ILingoKeyframe
    {
        public int Frame { get; set; }
        public LingoEaseType Ease { get; set; } = LingoEaseType.Linear;
    }
    public class LingoKeyFrame<T> : LingoKeyframe
    {
        public T Value { get; set; }
        

        public LingoKeyFrame(int frame, T value)
        {
            Frame = frame;
            Value = value;
        }
    }
}
