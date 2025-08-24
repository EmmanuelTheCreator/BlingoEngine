using System;
using System.IO;
using LingoEngine.Medias;
using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Medias;

public class SdlMemberMedia : ILingoFrameworkMemberMedia
{
    private readonly ISDLMediaPlayer _player;
    private LingoMemberMedia _member = null!;
    private LingoMediaStatus _status = LingoMediaStatus.Closed;

    public bool IsLoaded { get; private set; }
    public int Duration => (int)_player.Duration.TotalMilliseconds;
    public int CurrentTime
    {
        get => (int)_player.Position.TotalMilliseconds;
        set => _player.Seek(TimeSpan.FromMilliseconds(value));
    }
    public LingoMediaStatus MediaStatus => _status;

    public SdlMemberMedia(ISDLMediaPlayer player)
    {
        _player = player;
        _player.PlayStatusChanged += OnPlayStatusChanged;
        _player.MediaFinished += OnMediaFinished;
    }

    internal void Init(LingoMemberMedia member) => _member = member;

    public void Play() => _player.Play();
    public void Pause() => _player.Pause();
    public void Stop()
    {
        _player.Stop();
        _status = LingoMediaStatus.Closed;
    }

    public void Seek(int milliseconds) => _player.Seek(TimeSpan.FromMilliseconds(milliseconds));

    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void Preload()
    {
        if (IsLoaded)
            return;
        if (!string.IsNullOrEmpty(_member.FileName))
        {
            var fullPath = Path.GetFullPath(_member.FileName);
            _player.Load(fullPath);
            _status = LingoMediaStatus.Opened;
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        Stop();
        IsLoaded = false;
    }

    public bool IsPixelTransparent(int x, int y) => false;

    private void OnPlayStatusChanged(SdlMediaPlayerStatus status)
    {
        _status = status switch
        {
            SdlMediaPlayerStatus.Playing => LingoMediaStatus.Playing,
            SdlMediaPlayerStatus.Paused => LingoMediaStatus.Paused,
            _ => LingoMediaStatus.Closed
        };
    }

    private void OnMediaFinished()
    {
        _status = LingoMediaStatus.Closed;
        CurrentTime = 0;
    }
}
