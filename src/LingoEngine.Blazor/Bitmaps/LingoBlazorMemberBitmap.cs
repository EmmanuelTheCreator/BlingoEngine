using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Blazor.Util;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Tools;
using AbstUI.Tools;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Bitmaps;

/// <summary>
/// Minimal Blazor implementation of a bitmap cast member.
/// Currently provides basic file loading and metadata extraction so the
/// member can participate in the engine without a rendering backend.
/// </summary>
public class LingoBlazorMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
{
    private LingoMemberBitmap _member = null!;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<LingoSprite2D, IAbstUITextureUserSubscription> _spriteSubscriptions = new();
    private byte[]? _pixelData;
    private int _stride;
    private AbstBlazorTexture2D? _texture;
    private readonly Dictionary<LingoInkType, AbstBlazorTexture2D> _inkTextures = new();

    public byte[]? ImageData { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsLoaded { get; private set; }
    public IAbstTexture2D? TextureLingo => _texture;

    public LingoBlazorMemberBitmap(IJSRuntime js, AbstUIScriptResolver scripts, HttpClient httpClient)
    {
        _js = js;
        _scripts = scripts;
        _httpClient = httpClient;
    }

    internal void Init(LingoMemberBitmap member)
    {
        _member = member;
        if (!string.IsNullOrEmpty(member.FileName))
            Format = MimeHelper.GetMimeType(member.FileName);
    }

    public void Preload()
    {
        if (IsLoaded)
            return;
        if (!string.IsNullOrEmpty(_member.FileName))
        {
            try
            {
                var bytes = _httpClient.GetByteArrayAsync(_member.FileName).GetAwaiter().GetResult();
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
    public void ReleaseFromSprite(LingoSprite2D lingoSprite)
    {
        if (_spriteSubscriptions.TryGetValue(lingoSprite, out var sub))
        {
            sub.Release();
            _spriteSubscriptions.Remove(lingoSprite);
        }
    }

    internal void TrackSpriteUsage(LingoSprite2D sprite)
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
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            Width = img.Width;
            Height = img.Height;
            var rect = new Rectangle(0, 0, Width, Height);
            using var bmp = new Bitmap(img);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int rowBytes = Width * 4;
            _stride = rowBytes;
            _pixelData = new byte[rowBytes * Height];
            for (int y = 0; y < Height; y++)
            {
                var src = data.Scan0 + y * data.Stride;
                Marshal.Copy(src, _pixelData, y * rowBytes, rowBytes);
            }
            bmp.UnlockBits(data);
            // Convert BGRA -> RGBA
            for (int i = 0; i < _pixelData.Length; i += 4)
            {
                (_pixelData[i], _pixelData[i + 2]) = (_pixelData[i + 2], _pixelData[i]);
            }
        }
        catch { }
        _member.Size = bytes.Length;
        _member.Width = Width;
        _member.Height = Height;
    }

    public bool IsPixelTransparent(int x, int y)
        => PixelDataUtils.IsTransparent(_pixelData, _stride, Width, Height, x, y);

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
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

