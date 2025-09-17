using System;
using System.IO;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using UnityEngine;
using System.Threading.Tasks;

namespace BlingoEngine.Unity.Sounds;

/// <summary>
/// Unity implementation for sound cast members.
/// </summary>
public class UnityMemberSound : IBlingoFrameworkMemberSound, IDisposable
{
    private BlingoMemberSound _member = null!;

    internal AudioClip? Clip { get; private set; }
    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    internal void Init(BlingoMemberSound member)
    {
        _member = member;
    }

    public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
    public void CopyToClipboard() { }
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }

    public void Preload()
    {
        if (IsLoaded)
            return;
        Clip = LoadClip(_member.FileName, out var length, out var stereo, out var size);
        Length = length;
        Stereo = stereo;
        _member.Size = size;
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }

    public void Unload()
    {
        if (!IsLoaded)
            return;
        if (Clip != null)
        {
            UnityEngine.Object.Destroy(Clip);
            Clip = null;
        }
        _member.Size = 0;
        Length = 0;
        Stereo = false;
        IsLoaded = false;
    }

    public void Dispose() => Unload();

    internal static AudioClip? LoadClip(string path, out double length, out bool stereo, out int size)
    {
        length = 0;
        stereo = false;
        size = 0;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;
        var bytes = File.ReadAllBytes(path);
        size = bytes.Length;
        if (bytes.Length < 44)
            return null;
        int channels = BitConverter.ToInt16(bytes, 22);
        int sampleRate = BitConverter.ToInt32(bytes, 24);
        int bits = BitConverter.ToInt16(bytes, 34);
        int dataIndex = 44;
        for (int i = 12; i < bytes.Length - 8; i++)
        {
            if (bytes[i] == 'd' && bytes[i + 1] == 'a' && bytes[i + 2] == 't' && bytes[i + 3] == 'a')
            {
                dataIndex = i + 8;
                break;
            }
        }
        int bytesPerSample = Math.Max(1, bits / 8);
        int sampleCount = (bytes.Length - dataIndex) / bytesPerSample;
        length = sampleRate > 0 ? (double)sampleCount / sampleRate / Math.Max(1, channels) : 0;
        stereo = channels > 1;
        // AudioClip creation is not supported in this package; return null.
        return null;
    }

    public bool IsPixelTransparent(int x, int y) => false;
}

