using LingoEngine.Sounds;

namespace LingoEngine.Blazor.Sounds;

/// <summary>
/// Placeholder sound channel for the Blazor backend.
/// Implements the required interface without real audio output.
/// </summary>
public class LingoBlazorSoundChannel : ILingoFrameworkSoundChannel
{
    private LingoSoundChannel _channel = null!;

    public LingoBlazorSoundChannel(int number)
    {
        ChannelNumber = number;
    }

    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    public int Volume { get; set; }
    public int Pan { get; set; }
    public float CurrentTime { get; set; }
    public bool IsPlaying { get; private set; }
    public int ChannelCount => 2;
    public int SampleCount => 0;
    public float ElapsedTime => CurrentTime;
    public float StartTime { get; set; }
    public float EndTime { get; set; }

    internal void Init(LingoSoundChannel channel) => _channel = channel;

    public void Pause() => IsPlaying = !IsPlaying;
    public void PlayFile(string stringFilePath) => IsPlaying = true;
    public void PlayNow(LingoMemberSound member) => IsPlaying = true;
    public void Repeat() { }
    public void Rewind() => CurrentTime = 0;
    public void Stop()
    {
        IsPlaying = false;
        CurrentTime = 0;
    }
}
