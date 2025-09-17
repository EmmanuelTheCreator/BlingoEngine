using BlingoEngine.Animations;

namespace BlingoEngine.Movies;

/// <summary>
/// Generic helper to manage lists of keyframes with integer frames and values.
/// </summary>
internal abstract class BlingoKeyframeManager<TKeyFrame, TValue>
    where TKeyFrame : BlingoKeyframe
{
    private readonly List<TKeyFrame> _keyframes = new();

    public event Action? KeyframesChanged;

    public IReadOnlyList<TKeyFrame> Keyframes => _keyframes;

    protected abstract void SetValue(TKeyFrame kf, TValue value);
    protected abstract TKeyFrame Create(int frame, TValue value);

    public TKeyFrame Add(int frame, TValue value)
    {
        var kf = _keyframes.FirstOrDefault(k => k.Frame == frame);
        if (kf == null)
        {
            kf = Create(frame, value);
            _keyframes.Add(kf);
        }
        else
        {
            SetValue(kf, value);
        }
        KeyframesChanged?.Invoke();
        return kf;
    }

    public void Move(int previousFrame, int newFrame)
    {
        var kf = _keyframes.FirstOrDefault(k => k.Frame == previousFrame);
        if (kf != null)
        {
            kf.Frame = newFrame;
            KeyframesChanged?.Invoke();
        }
    }

    public void Remove(int frame)
    {
        var kf = _keyframes.FirstOrDefault(k => k.Frame == frame);
        if (kf != null)
        {
            _keyframes.Remove(kf);
            KeyframesChanged?.Invoke();
        }
    }
}

