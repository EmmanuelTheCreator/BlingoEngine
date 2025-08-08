using System;
using System.Collections.Generic;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Members;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Sprites;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Pictures;

public class SdlMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
{
    private LingoFilmLoopMember _member = null!;
    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public ILingoTexture2D? Texture { get; set; }
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;

    internal void Init(LingoFilmLoopMember member)
    {
        _member = member;
    }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void Preload()
    {
        IsLoaded = true;
    }

    public void Unload()
    {
        if (Texture is SdlTexture2D tex && tex.Texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Texture);
            Texture = null;
        }
        IsLoaded = false;
    }

    public void Erase()
    {
        Media = null;
        Unload();
    }

    public void ImportFileInto()
    {
        // not implemented
    }

    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        SdlClipboard.SetText(base64);
    }

    public void PasteClipboardInto()
    {
        var data = SdlClipboard.GetText();
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            Media = Convert.FromBase64String(data);
        }
        catch
        {
        }
    }

    public void ComposeTexture(LingoSprite2D hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers)
    {
        var sdlSprite = hostSprite.FrameworkObj as SdlSprite;
        if (sdlSprite == null)
            return;
        int width = (int)hostSprite.Width;
        int height = (int)hostSprite.Height;
        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA32);
        if (surface == nint.Zero)
            return;
        foreach (var layer in layers)
        {
            if (layer.Member is not LingoMemberBitmap pic)
                continue;
            var bmp = pic.Framework<SdlMemberBitmap>();
            var src = bmp.Surface;
            if (src == nint.Zero)
                continue;

            int destW = (int)layer.Width;
            int destH = (int)layer.Height;
            int srcW = bmp.Width;
            int srcH = bmp.Height;
            SDL.SDL_Rect srcRect;
            SDL.SDL_Rect dstRect;

            if (Framing == LingoFilmLoopFraming.Scale)
            {
                srcRect = new SDL.SDL_Rect { x = 0, y = 0, w = srcW, h = srcH };
                int x = (int)(layer.LocH + hostSprite.Width / 2f - destW / 2f);
                int y = (int)(layer.LocV + hostSprite.Height / 2f - destH / 2f);
                dstRect = new SDL.SDL_Rect { x = x, y = y, w = destW, h = destH };
                if (destW != srcW || destH != srcH)
                    SDL.SDL_BlitScaled(src, ref srcRect, surface, ref dstRect);
                else
                    SDL.SDL_BlitSurface(src, ref srcRect, surface, ref dstRect);
            }
            else
            {
                int cropW = Math.Min(destW, srcW);
                int cropH = Math.Min(destH, srcH);
                int cropX = (srcW - cropW) / 2;
                int cropY = (srcH - cropH) / 2;
                srcRect = new SDL.SDL_Rect { x = cropX, y = cropY, w = cropW, h = cropH };
                int x = (int)(layer.LocH + hostSprite.Width / 2f - cropW / 2f);
                int y = (int)(layer.LocV + hostSprite.Height / 2f - cropH / 2f);
                dstRect = new SDL.SDL_Rect { x = x, y = y, w = cropW, h = cropH };
                SDL.SDL_BlitSurface(src, ref srcRect, surface, ref dstRect);
            }
        }
        if (Texture is SdlTexture2D tex && tex.Texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Texture);
        }
        nint texture = SDL.SDL_CreateTextureFromSurface(sdlSprite.Renderer, surface);
        SDL.SDL_FreeSurface(surface);
        if (texture != nint.Zero)
            Texture = new SdlTexture2D(texture, width, height);
    }

    public void Dispose() { Unload(); }
}
