using AbstUI.SDL2.Medias;
using LibVLCSharp.Shared;

namespace AbstUI.SDL2.Vlc;

public sealed class VlcMediaPlayer : ISDLMediaPlayer, IDisposable
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;

    public event Action<SdlMediaPlayerStatus>? PlayStatusChanged;
    public event Action? MediaFinished;

    public TimeSpan Duration => TimeSpan.FromMilliseconds(_mediaPlayer.Length);
    public TimeSpan Position => TimeSpan.FromMilliseconds(_mediaPlayer.Time);

    public VlcMediaPlayer()
    {
        LibVLCSharp.Shared.Core.Initialize();
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);

        _mediaPlayer.EndReached += (_, _) => MediaFinished?.Invoke();
        _mediaPlayer.Playing += (_, _) => PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Playing);
        _mediaPlayer.Paused += (_, _) => PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Paused);
        _mediaPlayer.Stopped += (_, _) => PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Stopped);
    }

    public void Load(string path)
    {
        using var media = new Media(_libVlc, path, FromType.FromPath);
        _mediaPlayer.Media = media;
    }

    public void Play() => _mediaPlayer.Play();
    public void Pause() => _mediaPlayer.Pause();
    public void Stop() => _mediaPlayer.Stop();

    public void Seek(TimeSpan position) => _mediaPlayer.Time = (long)position.TotalMilliseconds;

    public void Dispose()
    {
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
    }
}
