using System;
using System.Collections.Generic;
using Godot;
using LingoEngine.Bitmaps;

namespace LingoEngine.LGodot.Bitmaps;

public class LingoGodotTexture2D : ILingoTexture2D
{
    private readonly Texture2D _texture;
    public Texture2D Texture => _texture;

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public LingoGodotTexture2D(Texture2D imageTexture)
    {
        _texture = imageTexture;
    }

    public int Width => _texture.GetWidth();

    public int Height => _texture._GetHeight();

    public ILingoTextureUserSubscription AddUser(object user)
    {
        var sub = new TextureSubscription(() => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0)
        {
            _texture.Dispose();
        }
    }

    private class TextureSubscription : ILingoTextureUserSubscription
    {
        private readonly Action _onRelease;
        public TextureSubscription(Action onRelease) => _onRelease = onRelease;
        public void Release() => _onRelease();
    }
}

