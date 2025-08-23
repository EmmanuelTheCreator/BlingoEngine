using AbstUI.LUnity.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Tools;
using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Tools;
using LingoEngine.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Unity.Bitmaps;

public class UnityMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
{
    private LingoMemberBitmap _member = null!;
    private Texture2D? _texture;
    private UnityTexture2D? _textureWrapper;
    private readonly Dictionary<LingoInkType, UnityTexture2D> _inkCache = new();
    private readonly ILogger<UnityMemberBitmap> _logger;

    public byte[]? ImageData { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsLoaded { get; private set; }

    internal Texture2D? TextureUnity => _texture;

    public IAbstTexture2D? TextureLingo => _textureWrapper;

    public UnityMemberBitmap(ILogger<UnityMemberBitmap> logger)
    {
        _logger = logger;
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
            if (File.Exists(_member.FileName))
            {
                var bytes = File.ReadAllBytes(_member.FileName);
                SetImageData(bytes);
            }
            else
            {
                _logger.LogWarning("MemberBitmap not found: {File}", _member.FileName);
            }
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        ClearCache();
        if (_textureWrapper != null)
        {
            _textureWrapper.Dispose();
            _textureWrapper = null;
        }
        else if (_texture != null)
        {
            UnityEngine.Object.Destroy(_texture);
        }
        _texture = null;
        IsLoaded = false;
    }

    public void Erase()
    {
        Unload();
        ImageData = null;
    }

    public void CopyToClipboard()
    {
        if (ImageData == null) return;
        var base64 = Convert.ToBase64String(ImageData);
        var text = "data:" + Format + ";base64," + base64;
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer",
            BindingFlags.Static | BindingFlags.Public);
        prop?.SetValue(null, text);
    }
    public void ImportFileInto() { }
    public void PasteClipboardInto()
    {
        var guiUtilityType = Type.GetType("UnityEngine.GUIUtility, UnityEngine");
        var prop = guiUtilityType?.GetProperty("systemCopyBuffer",
            BindingFlags.Static | BindingFlags.Public);
        var data = prop?.GetValue(null) as string;
        if (string.IsNullOrEmpty(data)) return;
        var parts = data.Split(',', 2);
        if (parts.Length != 2) return;
        var bytes = Convert.FromBase64String(parts[1]);
        SetImageData(bytes);
        Format = parts[0].Replace("data:", "");
    }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void SetImageData(byte[] bytes)
    {
        ImageData = bytes;
        if (_texture == null)
            _texture = new Texture2D(2, 2);
        _texture.LoadImage(bytes);
        Width = _texture.width;
        Height = _texture.height;
        _textureWrapper = new UnityTexture2D(_texture);
        _member.Size = bytes.Length;
        _member.Width = Width;
        _member.Height = Height;
    }

    public bool IsPixelTransparent(int x, int y)
    {
        if (_texture == null)
            return false;
        if (x < 0 || y < 0 || x >= _texture.width || y >= _texture.height)
            return true;
        var c = _texture.GetPixel(x, _texture.height - y - 1);
        return c.a <= 0f;
    }

    public void Dispose() => Unload();

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
        => GetTextureForInk(ink, transparentColor);

    private IAbstTexture2D? GetTextureForInk(LingoInkType ink, AColor backColor)
    {
        if (_texture == null)
            return null;

        if (!InkPreRenderer.CanHandle(ink))
        {
            _textureWrapper ??= new UnityTexture2D(_texture);
            return _textureWrapper;
        }

        var inkKey = InkPreRenderer.GetInkCacheKey(ink);
        if (_inkCache.TryGetValue(inkKey, out var cached))
            return cached;

        var colors = _texture.GetPixels32();
        var bytes = new byte[colors.Length * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            int j = i * 4;
            var c = colors[i];
            bytes[j] = c.r;
            bytes[j + 1] = c.g;
            bytes[j + 2] = c.b;
            bytes[j + 3] = c.a;
        }

        var data = InkPreRenderer.Apply(bytes, ink, backColor);
        var newTex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        newTex.LoadRawTextureData(data);
        newTex.Apply();
        var wrapper = new UnityTexture2D(newTex);
        _inkCache[inkKey] = wrapper;
        return wrapper;
    }

    private void ClearCache()
    {
        foreach (var tex in _inkCache.Values)
            tex.Dispose();
        _inkCache.Clear();
    }
}
