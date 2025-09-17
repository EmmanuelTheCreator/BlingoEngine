using System;
using System.Collections.Generic;
using System.Net.Http;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Blazor.Util;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Tools;
using AbstUI.Tools;
using Microsoft.JSInterop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace BlingoEngine.Blazor.Bitmaps;

/// <summary>
/// Minimal Blazor implementation of a bitmap cast member.
/// Currently provides basic file loading and metadata extraction so the
/// member can participate in the engine without a rendering backend.
/// </summary>
public class BlingoBlazorMemberBitmap : IBlingoFrameworkMemberBitmap, IDisposable
{
    private BlingoMemberBitmap _member = null!;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<BlingoSprite2D, IAbstUITextureUserSubscription> _spriteSubscriptions = new();
    private byte[]? _pixelData;
    private int _stride;
    private AbstBlazorTexture2D? _texture;
    private readonly Dictionary<BlingoInkType, AbstBlazorTexture2D> _inkTextures = new();

    public byte[]? ImageData { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsLoaded { get; private set; }
    public IAbstTexture2D? TextureBlingo => _texture;

    public BlingoBlazorMemberBitmap(IJSRuntime js, AbstUIScriptResolver scripts, HttpClient httpClient)
    {
        _js = js;
        _scripts = scripts;
        _httpClient = httpClient;
    }

    internal void Init(BlingoMemberBitmap member)
    {
        _member = member;
        if (!string.IsNullOrEmpty(member.FileName))
            Format = MimeHelper.GetMimeType(member.FileName);
    }

    public void Preload()
    {
        PreloadAsync().GetAwaiter().GetResult();
    }

    public async Task PreloadAsync()
    {
        if (IsLoaded)
            return;
        if (!string.IsNullOrEmpty(_member.FileName))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(_member.FileName);
                SetImageData(bytes);
            }
            catch { }
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        IsLoaded = false;
        _pixelData = null;
        _stride = 0;
        ClearCache();
    }

    public void Erase()
    {
        Unload();
        ImageData = null;
    }

    public void CopyToClipboard() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(BlingoSprite2D blingoSprite)
    {
        if (_spriteSubscriptions.TryGetValue(blingoSprite, out var sub))
        {
            sub.Release();
            _spriteSubscriptions.Remove(blingoSprite);
        }
    }

    internal void TrackSpriteUsage(BlingoSprite2D sprite)
    {
        if (_texture == null)
            return;
        if (!_spriteSubscriptions.ContainsKey(sprite))
            _spriteSubscriptions[sprite] = _texture.AddUser(sprite);
    }

    public void SetImageData(byte[] bytes)
    {
        ImageData = bytes;
        try
        {
            using var img = Image.Load<Rgba32>(bytes);
            Width = img.Width;
            Height = img.Height;
            _stride = Width * 4;
            _pixelData = new byte[_stride * Height];
            img.CopyPixelDataTo(_pixelData);
        }
        catch { }
        _member.Size = bytes.Length;
        _member.Width = Width;
        _member.Height = Height;
    }

    public bool IsPixelTransparent(int x, int y)
        => PixelDataUtils.IsTransparent(_pixelData, _stride, Width, Height, x, y);

    public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
    {
        if (_pixelData == null)
            return null;

        if (!InkPreRenderer.CanHandle(ink))
        {
            _texture ??= AbstBlazorTexture2D.CreateFromPixelDataAsync(_js, _scripts, _pixelData, Width, Height)
                .GetAwaiter().GetResult();
            return _texture;
        }

        var inkKey = InkPreRenderer.GetInkCacheKey(ink);
        if (_inkTextures.TryGetValue(inkKey, out var tex))
            return tex;

        var data = InkPreRenderer.Apply(_pixelData, ink, transparentColor);
        var newTex = AbstBlazorTexture2D.CreateFromPixelDataAsync(_js, _scripts, data, Width, Height)
            .GetAwaiter().GetResult();
        _inkTextures[inkKey] = newTex;
        return newTex;
    }

    private void ClearCache()
    {
        _texture?.Dispose();
        _texture = null;
        foreach (var tex in _inkTextures.Values)
            tex.Dispose();
        _inkTextures.Clear();
    }

    public void Dispose() => ClearCache();
}


