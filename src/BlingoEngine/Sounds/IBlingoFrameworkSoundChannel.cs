namespace BlingoEngine.Sounds
{
    /// <summary>
    /// Lingo Framework Sound Channel interface.
    /// </summary>
    public interface IBlingoFrameworkSoundChannel
    {
        int SampleRate { get; }
        int Volume { get; set; }
        int Pan { get; set; }
        float CurrentTime { get; set; }
        bool IsPlaying { get; }
        int ChannelCount { get; }
        int SampleCount { get; }
        float ElapsedTime { get; }
        float StartTime { get; set; }
        float EndTime { get; set; }

        void Pause();
        void PlayFile(string stringFilePath);
        void PlayNow(BlingoMemberSound member);
        void Repeat();
        void Rewind();
        void Stop();
    }
}

