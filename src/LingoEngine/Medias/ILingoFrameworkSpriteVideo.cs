
namespace LingoEngine.Medias
{
    /// <summary>
    /// Optional sprite capabilities for video playback.
    /// Framework sprite implementations may implement this to support media control.
    /// </summary>
    public interface ILingoFrameworkSpriteVideo
    {
        void Play();
        void Pause();
        void Stop();
        void Seek(int milliseconds);
        int Duration { get; }
        int CurrentTime { get; set; }
        LingoMediaStatus MediaStatus { get; }
    }
}
