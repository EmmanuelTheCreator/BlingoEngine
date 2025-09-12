using System.Runtime.InteropServices;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Bitmaps;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Sprites;
using AbstUI.Tools;
using System.Threading.Tasks;


namespace LingoEngine.SDL2.FilmLoops;

/// <summary>
/// SDL implementation of a film loop member. It composes the layers of a
/// <see cref="LingoFilmLoopMember"/> into an SDL texture for rendering.
/// </summary>
public class SdlMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
{
    ISdlRootComponentContext _sdlRootContext;
    private LingoFilmLoopMember _member = null!;
    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public IAbstTexture2D? TextureLingo { get; private set; }
    public APoint Offset { get; private set; }
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;


    public SdlMemberFilmLoop(ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
    }

    /// <summary>Initializes the film loop with its owning member.</summary>
    internal void Init(LingoFilmLoopMember member)
    {
        _member = member;
    }

    /// <summary>SDL film loops do not retain sprites, so this is a no-op.</summary>
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    /// <summary>Marks the film loop as loaded.</summary>
    public void Preload()
    {
        if (IsLoaded)
            return;
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }

    /// <summary>Releases any GPU resources held by the film loop.</summary>
    public void Unload()
    {
        if (TextureLingo is SdlTexture2D tex && tex.Handle != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Handle);
            TextureLingo = null;
        }
        IsLoaded = false;
    }

    /// <summary>Clears media data and unloads any cached texture.</summary>
    public void Erase()
    {
        Media = null;
        Unload();
    }

    /// <summary>Imports media from an external file. Not implemented.</summary>
    public void ImportFileInto()
    {
        // not implemented
    }

    /// <summary>Copies the encoded media to the clipboard.</summary>
    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        SdlClipboard.SetText(base64);
    }

    /// <summary>Pastes encoded media from the clipboard if available.</summary>
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
    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => TextureLingo;
    /// <summary>
    /// Composes the provided layers into a single SDL texture using the
    /// <see cref="LingoFilmLoopComposer"/> for layout and transforms.
    /// </summary>
    /// <returns>The composed texture for rendering.</returns>
    public IAbstTexture2D ComposeTexture(ILingoSprite2DLight hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers, int frame)
    {
        //return new SdlTexture2D(nint.Zero, 0, 0);
        //var sdlSprite = hostSprite.FrameworkObj as SdlSprite;
        //if (sdlSprite == null)
        //    return Texture ?? new SdlTexture2D(nint.Zero, 0, 0);

        var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
        Offset = prep.Offset;
        int width = prep.Width;
        int height = prep.Height;

        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        if (surface == nint.Zero)
            return TextureLingo ?? new SdlTexture2D(nint.Zero, width, height);
        var i = 0;
        foreach (var info in prep.Layers)
        {
            i++;
            nint srcImg = nint.Zero;
            SdlTexture2D? textureSdl = null;
            int widthS = 0;
            int heightS = 0;
            if (info.Sprite2D.Member is LingoMemberBitmap pic && pic.FrameworkObj is SdlMemberBitmap bmp)
            {
                textureSdl = ((ILingoFrameworkMemberBitmap)bmp).TextureLingo as SdlTexture2D;
                if (textureSdl == null) continue;
                textureSdl = bmp.GetTextureForInk(info.Ink, info.BackColor, _sdlRootContext.Renderer) as SdlTexture2D;
                if (textureSdl == null) continue;
                //DebugToDisk(textureSdl.Handle, $"filmloop_{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
                srcImg = textureSdl.Handle;
                widthS = textureSdl.Width;
                heightS = textureSdl.Height;
            }
            else
            {
                if (info.Sprite2D.Member != null)
                    info.Sprite2D.Member.Preload();
                if (info.Sprite2D.Texture == null) continue;

                textureSdl = (SdlTexture2D)info.Sprite2D.Texture;

                //DebugToDisk(textureSdl.Handle, $"filmloop_{frame}/{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
                srcImg = textureSdl.Handle;
                widthS = textureSdl.Width;
                heightS = textureSdl.Height;
            }
            if (srcImg == nint.Zero)
                continue;
            //DebugToDisk(srcImg, $"filmloop_{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
            nint srcSurf = textureSdl.ToSurface(_sdlRootContext.Renderer, out widthS, out heightS);
            bool freeSrc = true;
            var sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(srcSurf);
            var pixFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sSurf.format).format;
            if (pixFmt != SDL.SDL_PIXELFORMAT_ABGR8888)
            {
                nint conv = SDL.SDL_ConvertSurfaceFormat(srcSurf, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
                if (conv == nint.Zero)
                    continue;
                srcSurf = conv;
                sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(srcSurf);
                widthS = sSurf.w;
                heightS = sSurf.h;
                freeSrc = true;
            }

            if (info.SrcX != 0 || info.SrcY != 0 || info.SrcW != widthS || info.SrcH != heightS || info.DestW != info.SrcW || info.DestH != info.SrcH)
            {
                nint cropped = SDL.SDL_CreateRGBSurfaceWithFormat(0, info.DestW, info.DestH, 32, SDL.SDL_PIXELFORMAT_ABGR8888);
                if (cropped == nint.Zero)
                {
                    if (freeSrc)
                        SDL.SDL_FreeSurface(srcSurf);
                    continue;
                }
                SDL.SDL_Rect srect = new SDL.SDL_Rect { x = info.SrcX, y = info.SrcY, w = info.SrcW, h = info.SrcH };
                SDL.SDL_Rect drect = new SDL.SDL_Rect { x = 0, y = 0, w = info.DestW, h = info.DestH };
                if (Framing == LingoFilmLoopFraming.Scale)
                    SDL.SDL_BlitScaled(srcSurf, ref srect, cropped, ref drect);
                else
                    SDL.SDL_BlitSurface(srcSurf, ref srect, cropped, ref drect);
                if (freeSrc)
                    SDL.SDL_FreeSurface(srcSurf);
                srcSurf = cropped;
                freeSrc = true;
            }

            SdlSurfaceBlender.BlendSurface(surface, srcSurf, info.Transform.Matrix, info.Alpha);

            if (freeSrc)
                SDL.SDL_FreeSurface(srcSurf);
        }

        nint texture = SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, surface);
        //DebugToDisk(texture, $"filmloop_{frame}_{_member.Name}_{hostSprite.Name}");
        SDL.SDL_FreeSurface(surface);
        TextureLingo = new SdlTexture2D(texture, width, height, $"Filmloop_{frame}_Member_" + _member.Name);
        return TextureLingo;
    }
#if DEBUG
    public void DebugToDisk(nint texture, string filName) => SdlTexture2D.DebugToDisk(_sdlRootContext.Renderer, texture, filName);
#endif
    public void Dispose() { Unload(); }

    public bool IsPixelTransparent(int x, int y) => false;
}
