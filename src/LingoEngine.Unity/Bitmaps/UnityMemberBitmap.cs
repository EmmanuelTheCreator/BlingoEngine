using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Tools;
using System;
using System.IO;
using UnityEngine;

namespace LingoEngine.Unity.Bitmaps;

public class UnityMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
{
    private LingoMemberBitmap _member = null!;
    private Texture2D? _texture;
    private UnityImageTexture? _textureWrapper;

    public byte[]? ImageData { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsLoaded { get; private set; }

    public ILingoImageTexture? Texture => _textureWrapper;
    internal Texture2D? TextureUnity => _texture;

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
        if (!string.IsNullOrEmpty(_member.FileName) && File.Exists(_member.FileName))
        {
            var bytes = File.ReadAllBytes(_member.FileName);
            SetImageData(bytes);
        }
        IsLoaded = true;
    }

    public void Unload()
    {
        if (_texture != null)
        {
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
            _textureWrapper = null;
        }
        IsLoaded = false;
    }

    public void Erase()
    {
        Unload();
        ImageData = null;
    }

    public void CopyToClipboard() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    public void SetImageData(byte[] bytes)
    {
        ImageData = bytes;
        if (_texture == null)
            _texture = new Texture2D(2, 2);
        _texture.LoadImage(bytes);
        Width = _texture.width;
        Height = _texture.height;
        _textureWrapper = new UnityImageTexture(_texture);
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
}
