using LingoEngine.Medias;
using LingoEngine.Members;
using LingoEngine.Sprites;
using System.IO;

namespace LingoEngine.Blazor.Medias;

/// <summary>
/// Minimal Blazor implementation of a video media member.
/// Stores the media file path for use by the sprite.
/// </summary>
public class LingoBlazorMemberMedia : ILingoFrameworkMemberMedia
{
    private LingoMemberMedia _member = null!;

    internal string? Url { get; private set; }
    public bool IsLoaded { get; private set; }
    public int Duration { get; private set; }
    public int CurrentTime { get; set; }
    public LingoMediaStatus MediaStatus { get; private set; } = LingoMediaStatus.Closed;

    internal void Init(LingoMemberMedia member)
    {
        _member = member;
    }

    public void Play() => MediaStatus = LingoMediaStatus.Playing;
    public void Pause() => MediaStatus = LingoMediaStatus.Paused;
    public void Stop()
    {
        MediaStatus = LingoMediaStatus.Closed;
        CurrentTime = 0;
    }
    public void Seek(int milliseconds) => CurrentTime = milliseconds;

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
            Url = new System.Uri(fullPath).AbsoluteUri;
            MediaStatus = LingoMediaStatus.Opened;
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        IsLoaded = false;
        Url = null;
        MediaStatus = LingoMediaStatus.Closed;
    }

    public bool IsPixelTransparent(int x, int y) => false;
}
