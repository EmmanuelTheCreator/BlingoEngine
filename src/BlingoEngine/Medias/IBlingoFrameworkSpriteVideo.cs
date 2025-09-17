
namespace BlingoEngine.Medias
{
    /// <summary>
    /// Optional sprite capabilities for video playback.
    /// Framework sprite implementations may implement this to support media control.
    /// </summary>
    public interface IBlingoFrameworkSpriteVideo
    {
        void Play();
        void Pause();
        void Stop();
        void Seek(int milliseconds);
        int Duration { get; }
        int CurrentTime { get; set; }
        BlingoMediaStatus MediaStatus { get; }
    }
}

