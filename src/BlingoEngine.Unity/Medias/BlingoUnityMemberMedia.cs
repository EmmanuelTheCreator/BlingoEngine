using System;
using System.IO;
using BlingoEngine.Medias;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.Unity.Medias;

/// <summary>
/// Unity framework implementation for video media members.
/// Loads the media file path and exposes playback control information.
/// </summary>
public class BlingoUnityMemberMedia : IBlingoFrameworkMemberMedia
{
    private BlingoMemberMedia _member = null!;

    internal string? Url { get; private set; }
    public bool IsLoaded { get; private set; }
    public int Duration { get; private set; }
    public int CurrentTime { get; set; }
    public BlingoMediaStatus MediaStatus { get; private set; } = BlingoMediaStatus.Closed;

    internal void Init(BlingoMemberMedia member)
    {
        _member = member;
    }

    public void Play() => MediaStatus = BlingoMediaStatus.Playing;

    public void Pause() => MediaStatus = BlingoMediaStatus.Paused;

    public void Stop()
    {
        MediaStatus = BlingoMediaStatus.Closed;
        CurrentTime = 0;
    }

    public void Seek(int milliseconds) => CurrentTime = milliseconds;

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
            Url = new Uri(fullPath).AbsoluteUri;
            MediaStatus = BlingoMediaStatus.Opened;
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
        IsLoaded = false;
        Url = null;
        MediaStatus = BlingoMediaStatus.Closed;
    }

    public bool IsPixelTransparent(int x, int y) => false;
}

