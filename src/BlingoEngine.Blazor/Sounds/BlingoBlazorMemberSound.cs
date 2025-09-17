using System;
using System.Net.Http;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.Blazor.Sounds;

/// <summary>
/// Minimal Blazor implementation for sound cast members.
/// </summary>
public class BlingoBlazorMemberSound : IBlingoFrameworkMemberSound, IDisposable
{
    private readonly HttpClient _httpClient;
    private BlingoMemberSound _member = null!;
    private byte[]? _data;
    internal string? Url { get; private set; }

    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    public BlingoBlazorMemberSound(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    internal void Init(BlingoMemberSound member)
    {
        _member = member;
    }

    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
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

