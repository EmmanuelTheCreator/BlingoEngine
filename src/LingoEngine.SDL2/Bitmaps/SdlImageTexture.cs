using System;
using System.Collections.Generic;
using LingoEngine.Bitmaps;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Pictures;

public class SdlImageTexture : ILingoTexture2D
{
    private SDL.SDL_Surface _surfacePtr;
    public SDL.SDL_Surface Ptr => _surfacePtr;

    public nint SurfaceId { get; private set; }
    public int Width { get; set; }

    public int Height { get; set; }

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public SdlImageTexture(SDL.SDL_Surface surfacePtr, nint surfaceId, int width, int height)
    {
        _surfacePtr = surfacePtr;
        SurfaceId = surfaceId;
        Width = width;
        Height = height;
    }

    public ILingoTextureUserSubscription AddUser(object user)
    {
        var sub = new TextureSubscription(() => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && SurfaceId != nint.Zero)
        {
            SDL.SDL_FreeSurface(SurfaceId);
            SurfaceId = nint.Zero;
        }
    }

    private class TextureSubscription : ILingoTextureUserSubscription
    {
        private readonly Action _onRelease;
        public TextureSubscription(Action onRelease) => _onRelease = onRelease;
        public void Release() => _onRelease();
    }
}

public class SdlTexture2D : ILingoTexture2D
{
    public nint Texture { get; private set; }
    public int Width { get; }
    public int Height { get; }

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public SdlTexture2D(nint texture, int width, int height)
    {
        Texture = texture;
        Width = width;
        Height = height;
    }

    public ILingoTextureUserSubscription AddUser(object user)
    {
        var sub = new TextureSubscription(() => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && Texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(Texture);
            Texture = nint.Zero;
        }
    }

    private class TextureSubscription : ILingoTextureUserSubscription
    {
        private readonly Action _onRelease;
        public TextureSubscription(Action onRelease) => _onRelease = onRelease;
        public void Release() => _onRelease();
    }
}

