using System;
using System.IO;
using AbstUI.SDL2.Medias;
using BlingoEngine.Medias;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.SDL2.Medias;

public class SdlMemberMedia : IBlingoFrameworkMemberMedia
{
    private readonly ISDLMediaPlayer _player;
    private BlingoMemberMedia _member = null!;
    private BlingoMediaStatus _status = BlingoMediaStatus.Closed;

    public bool IsLoaded { get; private set; }
    public int Duration => (int)_player.Duration.TotalMilliseconds;
    public int CurrentTime
    {
        get => (int)_player.Position.TotalMilliseconds;
        set => _player.Seek(TimeSpan.FromMilliseconds(value));
    }
    public BlingoMediaStatus MediaStatus => _status;

    public SdlMemberMedia(ISDLMediaPlayer player)
    {
        _player = player;
        _player.PlayStatusChanged += OnPlayStatusChanged;
        _player.MediaFinished += OnMediaFinished;
    }

    internal void Init(BlingoMemberMedia member) => _member = member;

    public void Play() => _player.Play();
    public void Pause() => _player.Pause();
    public void Stop()
    {
        _player.Stop();
        _status = BlingoMediaStatus.Closed;
    }

    public void Seek(int milliseconds) => _player.Seek(TimeSpan.FromMilliseconds(milliseconds));

    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }

    public void Preload()
    {
        if (IsLoaded)
            return;
        if (!string.IsNullOrEmpty(_member.FileName))
        {
            var fullPath = Path.GetFullPath(_member.FileName);
            _player.Load(fullPath);
            _status = BlingoMediaStatus.Opened;
        }
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
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
            SdlMediaPlayerStatus.Playing => BlingoMediaStatus.Playing,
            SdlMediaPlayerStatus.Paused => BlingoMediaStatus.Paused,
            _ => BlingoMediaStatus.Closed
        };
    }

    private void OnMediaFinished()
    {
        _status = BlingoMediaStatus.Closed;
        CurrentTime = 0;
    }
}

