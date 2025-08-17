using AbstUI.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Bitmaps;

/// <summary>
/// Texture backed by an off-screen HTML &lt;canvas&gt; element.
/// </summary>
public class AbstBlazorTexture2D : IAbstTexture2D
{
    private readonly IJSRuntime _jsRuntime;
    public ElementReference Canvas { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsDisposed { get; private set; }
    public string Name { get; set; } = string.Empty;

    private readonly Dictionary<object, TextureSubscription> _users = new();

    protected AbstBlazorTexture2D(IJSRuntime jsRuntime, ElementReference canvas, int width, int height, string name = "")
    {
        _jsRuntime = jsRuntime;
        Canvas = canvas;
        Width = width;
        Height = height;
        Name = name;
    }

    public static async Task<AbstBlazorTexture2D> CreateAsync(IJSRuntime jsRuntime, int width, int height, string name = "")
    {
        var canvas = await jsRuntime.InvokeAsync<ElementReference>("abstCanvas.createCanvas", width, height);
        return new AbstBlazorTexture2D(jsRuntime, canvas, width, height, name);
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
        _ = _jsRuntime.InvokeVoidAsync("abstCanvas.disposeCanvas", Canvas);
    }

    private class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public IAbstTexture2D Texture { get; }
        public TextureSubscription(IAbstTexture2D texture, Action onRelease)
        {
            Texture = texture;
            _onRelease = onRelease;
        }

        public void Release() => _onRelease();
    }
}
