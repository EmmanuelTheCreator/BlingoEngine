using LingoEngine.Sounds;

namespace LingoEngine.Unity.Sounds;

/// <summary>
/// Stub sound channel for Unity backend.
/// </summary>
public class UnitySoundChannel : ILingoFrameworkSoundChannel
{
    private LingoSoundChannel _lingoSoundChannel = null!;

    public UnitySoundChannel(int number)
    {
        ChannelNumber = number;
    }

    internal void Init(LingoSoundChannel channel)
    {
        _lingoSoundChannel = channel;
    }

    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    public int Volume { get; set; }
    public int Pan { get; set; }
    public float CurrentTime { get; set; }
    public bool IsPlaying { get; private set; }
    public int ChannelCount => 2;
    public int SampleCount => 0;
    public float ElapsedTime => 0;
    public float StartTime { get; set; }
    public float EndTime { get; set; }

    public void Pause() { }
    public void PlayFile(string stringFilePath) { IsPlaying = true; }
    public void PlayNow(LingoMemberSound member) { IsPlaying = true; }
    public void Repeat() { }
    public void Rewind() { }
    public void Stop() { IsPlaying = false; }
}
