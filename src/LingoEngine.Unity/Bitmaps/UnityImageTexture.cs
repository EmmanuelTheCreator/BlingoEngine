using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using UnityEngine;

namespace LingoEngine.Unity.Bitmaps;

public class UnityTexture2D : ILingoTexture2D
{
    public Texture2D? Texture { get; private set; }

    private readonly Dictionary<object, TextureSubscription> _users = new();
    public string Name { get; set; } = string.Empty;

    public UnityTexture2D(Texture2D texture, string name = "")
    {
        Texture = texture;
        Name = name;
    }

    public int Width => Texture!.width;
    public int Height => Texture!.height;

    public bool IsDisposed => throw new NotImplementedException();


    public IAbstUITextureUserSubscription AddUser(object user)
    {
        var sub = new TextureSubscription(this, () => RemoveUser(user));
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

    private class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public IAbstUITexture2D Texture { get; }
        public TextureSubscription(ILingoTexture2D texture, Action onRelease)
        {
            _onRelease = onRelease;
            Texture = texture;
        }


        public void Release() => _onRelease();
    }
}
