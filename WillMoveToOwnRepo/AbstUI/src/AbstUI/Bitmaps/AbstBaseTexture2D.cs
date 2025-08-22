using System;
using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.Bitmaps;

public abstract class AbstBaseTexture2D<TFrameworkTexture> : IAbstTexture2D
{
    private readonly Dictionary<object, TextureSubscription> _users = new();

    protected AbstBaseTexture2D(string name = "")
    {
        Name = name;
    }

    public abstract int Width { get; }
    public abstract int Height { get; }
    public bool IsDisposed { get; private set; }
    public string Name { get; set; }

    public IAbstUITextureUserSubscription AddUser(object user)
    {
        if (IsDisposed)
            throw new Exception("Texture is disposed and cannot be used anymore.");
        var sub = new TextureSubscription(this, () => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    protected virtual void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && !IsDisposed)
            Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        DisposeTexture();
    }

    protected abstract void DisposeTexture();

    protected class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public AbstBaseTexture2D<TFrameworkTexture> Texture { get; }
        IAbstTexture2D IAbstUITextureUserSubscription.Texture => Texture;

        public TextureSubscription(AbstBaseTexture2D<TFrameworkTexture> texture, Action onRelease)
        {
            Texture = texture;
            _onRelease = onRelease;
        }

        public void Release() => _onRelease();
    }
}

