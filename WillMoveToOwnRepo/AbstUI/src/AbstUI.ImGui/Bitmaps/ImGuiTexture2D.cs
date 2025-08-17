using System;
using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.ImGui.Bitmaps;

/// <summary>
/// Minimal texture wrapper with no native dependencies.
/// </summary>
public class ImGuiTexture2D : IAbstTexture2D
{
    public nint Handle { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public bool IsDisposed { get; private set; }
    public string Name { get; set; } = "";

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public ImGuiTexture2D(nint texture, int width, int height, string name = "")
    {
        Handle = texture;
        Width = width;
        Height = height;
        Name = name;
    }

    public IAbstUITextureUserSubscription AddUser(object user)
    {
        if (IsDisposed) throw new Exception("Texture is disposed and cannot be used anymore.");
        var sub = new TextureSubscription(this, () => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && Handle != nint.Zero)
            Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        Handle = nint.Zero;
        // TODO: release texture resources
    }

    public nint ToSurface(nint renderer, out int w, out int h, uint? fmt = null)
    {
        w = Width;
        h = Height;
        // TODO: convert texture to a CPU surface without platform APIs
        return nint.Zero;
    }

    public IAbstTexture2D Clone(nint renderer)
        => throw new NotImplementedException();

#if DEBUG
    public void DebugWriteToDisk(nint renderer) => throw new NotImplementedException();
    public static void DebugToDisk(nint renderer, nint texture, string fileName) => throw new NotImplementedException();
    public static void DebugToDisk(nint renderer, nint texture, string folder, string fileName) => throw new NotImplementedException();
#endif

    private class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public IAbstTexture2D Texture { get; }

        public TextureSubscription(ImGuiTexture2D texture, Action onRelease)
        {
            Texture = texture;
            _onRelease = onRelease;
        }

        public void Release() => _onRelease();
    }
}

