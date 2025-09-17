using System.IO;
using System.Runtime.InteropServices;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using AbstUI.SDL2.SDLL;
using System.Threading.Tasks;

namespace BlingoEngine.SDL2.Sounds;

public class SdlMemberSound : IBlingoFrameworkMemberSound, IDisposable
{
    private BlingoMemberSound _member = null!;
    private nint _chunk = nint.Zero;
    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    internal void Init(BlingoMemberSound member)
    {
        _member = member;
    }
    public void Dispose() { Unload(); }

    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
    public void CopyToClipboard() { }
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload()
    {
        if (_member == null) return;// need to be initialized first
        if (IsLoaded) return;
        if (!File.Exists(_member.FileName))
            return;
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_FreeChunk(_chunk);
            _chunk = nint.Zero;
        }
        _chunk = SDL_mixer.Mix_LoadWAV(_member.FileName);
        if (_chunk == nint.Zero)
            return;

        var fi = new FileInfo(_member.FileName);
        _member.Size = fi.Length;
        Length = fi.Length / 44100.0;
        Stereo = true;
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }
    public void Unload()
    {
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_FreeChunk(_chunk);
            _chunk = nint.Zero;
        }
        IsLoaded = false;
    }

    public bool IsPixelTransparent(int x, int y) => false;
}

