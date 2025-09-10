using AbstUI.SDL2.Medias;
using FFMpegCore;

namespace AbstUI.SDL2.FFmpeg;

public sealed class FfmpegMediaPlayer : ISDLMediaPlayer, IDisposable
{
    private string? _source;
    private TimeSpan _duration;
    private Task? _playTask;

    public event Action<SdlMediaPlayerStatus>? PlayStatusChanged;
    public event Action? MediaFinished;

    public TimeSpan Duration => _duration;
    public TimeSpan Position => TimeSpan.Zero;

    public void Load(string path)
    {
        _source = path;
        var analysis = FFProbe.Analyse(path);
        _duration = analysis.Duration;
    }

    public void Play()
    {
        if (_source == null)
            return;

        PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Playing);
        _playTask = Task.Run(async () =>
        {
            await FFMpegArguments
                .FromFileInput(_source)
                .OutputToFile("-", false, options => options.WithCustomArgument("-f null"))
                .ProcessAsynchronously(false);

            MediaFinished?.Invoke();
            PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Stopped);
        });
    }

    public void Pause()
    {
        PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Paused);
    }

    public void Stop()
    {
        PlayStatusChanged?.Invoke(SdlMediaPlayerStatus.Stopped);
    }

    public void Seek(TimeSpan position)
    {
        // Seeking not implemented in this lightweight wrapper
    }

    public void Dispose()
    {
        _playTask?.Dispose();
    }
}
