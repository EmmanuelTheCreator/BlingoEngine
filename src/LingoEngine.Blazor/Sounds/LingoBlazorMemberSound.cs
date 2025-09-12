using System;
using System.Net.Http;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.Blazor.Sounds;

/// <summary>
/// Minimal Blazor implementation for sound cast members.
/// </summary>
public class LingoBlazorMemberSound : ILingoFrameworkMemberSound, IDisposable
{
    private readonly HttpClient _httpClient;
    private LingoMemberSound _member = null!;
    private byte[]? _data;
    internal string? Url { get; private set; }

    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    public LingoBlazorMemberSound(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    internal void Init(LingoMemberSound member)
    {
        _member = member;
    }

    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void CopyToClipboard() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }

    public void Erase() => Unload();

    public void Preload() => PreloadAsync().GetAwaiter().GetResult();

    public async Task PreloadAsync()
    {
        if (IsLoaded)
            return;
        if (!string.IsNullOrEmpty(_member.FileName))
        {
            Url = _member.FileName;
            try
            {
                _data = await _httpClient.GetByteArrayAsync(_member.FileName);
                _member.Size = _data.Length;
                Length = _data.Length / 44100.0;
                Stereo = true;
            }
            catch { }
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        IsLoaded = false;
        Stereo = false;
        Length = 0;
        _data = null;
    }

    public void Dispose() => Unload();
    public bool IsPixelTransparent(int x, int y) => false;
}
