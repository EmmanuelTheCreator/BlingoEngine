#if NET48
namespace System
{
    public readonly struct Index
    {
        private readonly int _value;
        public Index(int value)
        {
            _value = value;
        }
        public int GetOffset(int length) => _value;
        public static implicit operator Index(int value) => new Index(value);
    }

    public readonly struct Range
    {
        public Index Start { get; }
        public Index End { get; }
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }
    }
}
#endif
