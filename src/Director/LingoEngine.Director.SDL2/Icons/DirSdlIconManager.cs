using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using LingoEngine.Director.Core.Icons;
using Microsoft.Extensions.Logging;
using LingoEngine.SDL2;

namespace LingoEngine.Director.SDL2.Icons
{
    public class LingoIconSheetSdl : LingoIconSheet<SdlTexture2D>
    {
        public LingoIconSheetSdl(SdlTexture2D image, int iconWidth, int iconHeight, int horizontalSpacing, int iconCount)
            : base(image, iconWidth, iconHeight, horizontalSpacing, iconCount)
        {
        }
    }

    public class DirSdlIconManager : DirectorIconManager<LingoIconSheetSdl>
    {
        private readonly ILogger _logger;
        private readonly nint _renderer;

        public DirSdlIconManager(ILogger<DirSdlIconManager> logger, LingoSdlRootContext context)
        {
            _logger = logger;
            _renderer = context.Renderer;
        }

        protected override LingoIconSheetSdl? OnLoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            var texture = SDL_image.IMG_LoadTexture(_renderer, path);
            if (texture == nint.Zero)
            {
                _logger.LogWarning($"Failed to load texture: {path}");
                return null;
            }
            SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);
            var sdlTexture = new SdlTexture2D(texture, w, h, path);
            return new LingoIconSheetSdl(sdlTexture, iconWidth, iconHeight, horizontalSpacing, itemCount);
        }

        protected override IAbstTexture2D? OnGetTextureImage(LingoIconSheetSdl sheet, int x)
        {
            var dst = SDL.SDL_CreateTexture(_renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, sheet.IconWidth, sheet.IconHeight);
            if (dst == nint.Zero)
                return null;
            var prev = SDL.SDL_GetRenderTarget(_renderer);
            SDL.SDL_SetRenderTarget(_renderer, dst);
            var src = new SDL.SDL_Rect { x = x, y = 0, w = sheet.IconWidth, h = sheet.IconHeight };
            var dest = new SDL.SDL_Rect { x = 0, y = 0, w = sheet.IconWidth, h = sheet.IconHeight };
            SDL.SDL_RenderCopy(_renderer, sheet.Image.Handle, ref src, ref dest);
            SDL.SDL_SetRenderTarget(_renderer, prev);
            return new SdlTexture2D(dst, sheet.IconWidth, sheet.IconHeight);
        }
    }
}
