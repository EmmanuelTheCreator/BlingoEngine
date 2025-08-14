using System;
using System.Collections.Generic;
using LingoEngine.Bitmaps;
using UnityEngine;

namespace LingoEngine.Unity.Bitmaps;

public class UnityTexture2D : ILingoTexture2D
{
    public Texture2D? Texture { get; private set; }

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public UnityTexture2D(Texture2D texture)
    {
        Texture = texture;
    }

    public int Width => Texture!.width;
    public int Height => Texture!.height;

    public bool IsDisposed => throw new NotImplementedException();

    public ILingoTextureUserSubscription AddUser(object user)
    {
        var sub = new TextureSubscription(() => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && Texture != null)
        {
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private class TextureSubscription : ILingoTextureUserSubscription
    {
        private readonly Action _onRelease;
        public TextureSubscription(Action onRelease) => _onRelease = onRelease;
        public void Release() => _onRelease();
    }
}
