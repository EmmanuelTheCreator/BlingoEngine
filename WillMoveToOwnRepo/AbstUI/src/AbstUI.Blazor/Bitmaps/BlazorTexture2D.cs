using Microsoft.AspNetCore.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Bitmaps;

/// <summary>
/// Simple texture wrapper backed by a DOM element.
/// </summary>
public class BlazorTexture2D : IAbstTexture2D
{
    /// <summary>Reference to an HTML image or canvas element representing the texture.</summary>
    public ElementReference Element { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsDisposed { get; private set; }
    public string Name { get; set; } = string.Empty;

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public BlazorTexture2D(ElementReference element, int width, int height, string name = "")
    {
        Element = element;
        Width = width;
        Height = height;
        Name = name;
    }

    public IAbstUITextureUserSubscription AddUser(object user)
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
    }

    private class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public IAbstTexture2D Texture { get; }
        public TextureSubscription(BlazorTexture2D texture, Action onRelease)
        {
            Texture = texture;
            _onRelease = onRelease;
        }

        public void Release() => _onRelease();
    }
}
