using Godot;
using LingoEngine.Bitmaps;

namespace LingoEngine.LGodot.Bitmaps;

public class LingoGodotTexture2D : ILingoTexture2D
{
    private readonly Texture2D _texture;
    public Texture2D Texture => _texture;

    private readonly Dictionary<object, TextureSubscription> _users = new();
    public string Name { get; set; } = "";
    public LingoGodotTexture2D(Texture2D imageTexture)
    {
        _texture = imageTexture;
    }

    public int Width => _texture.GetWidth();

    public int Height => _texture._GetHeight();

    public bool IsDisposed { get; private set; }

    public ILingoTextureUserSubscription AddUser(object user)
    {
        if (IsDisposed)
            throw new Exception("Texture is disposed and cannot be used anymore.");
        var sub = new TextureSubscription(this, () => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
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
        _texture.Dispose();
    }
    public ILingoTexture2D Clone()
    {
        // Get the pixel data from the existing texture
        Image img = _texture.GetImage(); // This returns a copy of the image data
        ImageTexture newTex = ImageTexture.CreateFromImage(img);

        return new LingoGodotTexture2D(newTex);
    }
    private class TextureSubscription : ILingoTextureUserSubscription
    {
        private readonly Action _onRelease;
        public ILingoTexture2D Texture { get; }
        public TextureSubscription(ILingoTexture2D texture, Action onRelease)
        {
            _onRelease = onRelease;
            Texture = texture;
        }

        public void Release() => _onRelease();
    }
}

