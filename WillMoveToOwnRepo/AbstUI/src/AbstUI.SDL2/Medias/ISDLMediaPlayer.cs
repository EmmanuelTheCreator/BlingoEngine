namespace AbstUI.SDL2.Medias;

public enum SdlMediaPlayerStatus
{
    Stopped,
    Playing,
    Paused
}

public interface ISDLMediaPlayer
{
    event Action<SdlMediaPlayerStatus>? PlayStatusChanged;
    event Action? MediaFinished;

    void Load(string path);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);

    TimeSpan Duration { get; }
    TimeSpan Position { get; }
}
