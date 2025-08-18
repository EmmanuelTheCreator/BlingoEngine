using System;
using System.Runtime.InteropServices;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Bitmaps;

public static class SdlTexture2DExtensions
{
    /// <summary>
    /// Converts the SDL texture to a PNG and returns its Base64 representation.
    /// </summary>
    /// <param name="texture">Texture to encode.</param>
    /// <param name="renderer">Renderer used to read the texture pixels.</param>
    public static string ToPngBase64(this SdlTexture2D texture, nint renderer)
    {
        nint surface = texture.ToSurface(renderer, out int w, out int h);
        try
        {
            int bufferSize = w * h * 4 + 1024; // raw pixel data + small overhead
            nint buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                nint rw = SDL.SDL_RWFromMem(buffer, bufferSize);
                if (rw == nint.Zero)
                    throw new Exception(SDL.SDL_GetError());

                try
                {
                    if (SDL_image.IMG_SavePNG_RW(surface, rw, 0) != 0)
                        throw new Exception($"IMG_SavePNG_RW failed: {SDL_image.IMG_GetError()}");

                    long size = SDL.SDL_RWtell(rw);
                    byte[] bytes = new byte[size];
                    Marshal.Copy(buffer, bytes, 0, (int)size);
                    return Convert.ToBase64String(bytes);
                }
                finally
                {
                    SDL.SDL_FreeRW(rw);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            SDL.SDL_FreeSurface(surface);
        }
    }
}
