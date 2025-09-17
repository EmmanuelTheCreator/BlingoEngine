using System;
using System.Runtime.InteropServices;
using BlingoEngine.Primitives;
using BlingoEngine.Shapes;
using BlingoEngine.Sprites;
using BlingoEngine.Bitmaps;
using AbstUI.Tools;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Core;
using System.Threading.Tasks;

namespace BlingoEngine.SDL2.Shapes
{
    public class SdlMemberShape : IBlingoFrameworkMemberShape, IDisposable
    {

        private readonly ISdlRootComponentContext _sdlRootContext;
        private nint _surface = nint.Zero;
        private SDL.SDL_Surface _surfacePtr;
        internal nint Surface => _surface;
        private SdlTexture2D? _texture;
        public IAbstTexture2D? TextureBlingo => _texture;
        public bool IsLoaded { get; private set; }
        public BlingoList<APoint> VertexList { get; } = new();
        public BlingoShapeType ShapeType { get; set; } = BlingoShapeType.Rectangle;
        public AColor FillColor { get; set; } = AColor.FromRGB(255, 255, 255);
        public AColor EndColor { get; set; } = AColor.FromRGB(255, 255, 255);
        public AColor StrokeColor { get; set; } = AColor.FromRGB(0, 0, 0);
        public int StrokeWidth { get; set; } = 1;
        public bool Closed { get; set; } = true;
        public bool AntiAlias { get; set; } = true;
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Filled { get; set; } = true;





        public SdlMemberShape(ISdlRootComponentContext sdlRootContext)
        {
            _sdlRootContext = sdlRootContext;
        }


        public void CopyToClipboard() { }
        public void Erase() { VertexList.Clear(); }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }

        public unsafe void Preload()
        {
            if (IsLoaded)
                return;

            int w = Math.Max(1, (int)Width);
            int h = Math.Max(1, (int)Height);

            _surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, w, h, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
            if (_surface == nint.Zero)
                return;

            _surfacePtr = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
            uint fill = SDL.SDL_MapRGBA(_surfacePtr.format, FillColor.R, FillColor.G, FillColor.B, FillColor.A);
            uint stroke = SDL.SDL_MapRGBA(_surfacePtr.format, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);

            switch (ShapeType)
            {
                case BlingoShapeType.Rectangle:
                    DrawRectangle(w, h, fill, stroke);
                    break;
                case BlingoShapeType.Oval:
                    DrawOval(w, h, fill, stroke);
                    break;
                case BlingoShapeType.Line:
                    DrawLineShape(w, h, stroke);
                    break;
                case BlingoShapeType.PolyLine:
                    DrawPolyLineShape(w, h, stroke);
                    break;
                case BlingoShapeType.RoundRect:
                    DrawRoundRect(w, h, fill, stroke);
                    break;
            }
            _texture = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), w, h);
            IsLoaded = true;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            if (_surface != nint.Zero)
            {
                SDL.SDL_FreeSurface(_surface);
                _surface = nint.Zero;
            }
            IsLoaded = false;
        }

        public void Dispose()
        {
            Unload();
        }

        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }


        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
        {

            if (_texture == null)
                Preload();
            else if (_texture.IsDisposed)
            {
                _texture = null;
                IsLoaded = false;
                Preload();
            }
            return _texture?.Clone(_sdlRootContext.Renderer);
        }


        #region Draw methods

        private unsafe void DrawRectangle(int w, int h, uint fill, uint stroke)
        {
            SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
            if (Filled)
                SDL.SDL_FillRect(_surface, ref rect, fill);
            if (!Filled || StrokeWidth > 0)
            {
                SDL.SDL_Rect top = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = StrokeWidth };
                SDL.SDL_Rect bottom = new SDL.SDL_Rect { x = 0, y = h - StrokeWidth, w = w, h = StrokeWidth };
                SDL.SDL_Rect left = new SDL.SDL_Rect { x = 0, y = 0, w = StrokeWidth, h = h };
                SDL.SDL_Rect right = new SDL.SDL_Rect { x = w - StrokeWidth, y = 0, w = StrokeWidth, h = h };
                SDL.SDL_FillRect(_surface, ref top, stroke);
                SDL.SDL_FillRect(_surface, ref bottom, stroke);
                SDL.SDL_FillRect(_surface, ref left, stroke);
                SDL.SDL_FillRect(_surface, ref right, stroke);
            }
        }

        private unsafe void DrawOval(int w, int h, uint fill, uint stroke)
        {
            SDL.SDL_LockSurface(_surface);
            byte* pix = (byte*)_surfacePtr.pixels;
            int pitch = _surfacePtr.pitch;
            float cx = w / 2f;
            float cy = h / 2f;
            float rx = w / 2f;
            float ry = h / 2f;
            float maxR = MathF.Max(rx, ry);

            int totalPixels = w * h;
            ParallelHelper.For(0, h, totalPixels, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (Filled && d <= 1f)
                        SetPixel(pix, pitch, x, y, fill);
                    else if (!Filled && Math.Abs(d - 1f) <= StrokeWidth / maxR)
                        SetPixel(pix, pitch, x, y, stroke);
                }
            });
            SDL.SDL_UnlockSurface(_surface);
        }

        private unsafe void DrawLineShape(int w, int h, uint stroke)
        {
            if (VertexList.Count < 2)
                return;
            SDL.SDL_LockSurface(_surface);
            byte* pix = (byte*)_surfacePtr.pixels;
            int pitch = _surfacePtr.pitch;
            var p0 = VertexList[0];
            var p1 = VertexList[1];
            DrawLine(pix, pitch, w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
            SDL.SDL_UnlockSurface(_surface);
        }

        private unsafe void DrawPolyLineShape(int w, int h, uint stroke)
        {
            if (VertexList.Count < 2)
                return;
            SDL.SDL_LockSurface(_surface);
            byte* pix = (byte*)_surfacePtr.pixels;
            int pitch = _surfacePtr.pitch;
            for (int i = 0; i < VertexList.Count - 1; i++)
            {
                var p0 = VertexList[i];
                var p1 = VertexList[i + 1];
                DrawLine(pix, pitch, w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
            }
            if (Closed)
            {
                var p0 = VertexList[^1];
                var p1 = VertexList[0];
                DrawLine(pix, pitch, w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
            }
            SDL.SDL_UnlockSurface(_surface);
        }

        private unsafe void DrawRoundRect(int w, int h, uint fill, uint stroke)
        {
            int r = Math.Min(Math.Min(w, h) / 5, 20);
            SDL.SDL_LockSurface(_surface);
            byte* pix = (byte*)_surfacePtr.pixels;
            int pitch = _surfacePtr.pitch;
            int totalPixels = w * h;
            ParallelHelper.For(0, h, totalPixels, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    bool inside = true;
                    if (x < r && y < r)
                        inside = (x - r) * (x - r) + (y - r) * (y - r) <= r * r;
                    else if (x >= w - r && y < r)
                        inside = (x - (w - r - 1)) * (x - (w - r - 1)) + (y - r) * (y - r) <= r * r;
                    else if (x < r && y >= h - r)
                        inside = (x - r) * (x - r) + (y - (h - r - 1)) * (y - (h - r - 1)) <= r * r;
                    else if (x >= w - r && y >= h - r)
                        inside = (x - (w - r - 1)) * (x - (w - r - 1)) + (y - (h - r - 1)) * (y - (h - r - 1)) <= r * r;

                    if (Filled)
                    {
                        if (inside)
                            SetPixel(pix, pitch, x, y, fill);
                        else if (Math.Abs((x - r) * (x - r) + (y - r) * (y - r) - r * r) < r && x < r && y < r)
                            SetPixel(pix, pitch, x, y, stroke);
                    }
                    else if (!inside && ((x >= r && x < w - r) || (y >= r && y < h - r)))
                    {
                        SetPixel(pix, pitch, x, y, stroke);
                    }
                }
            });
            SDL.SDL_UnlockSurface(_surface);
        }

        private unsafe static void SetPixel(byte* pix, int pitch, int x, int y, uint color)
        {
            byte* p = pix + y * pitch + x * 4;
            p[0] = (byte)(color & 0xFF);
            p[1] = (byte)((color >> 8) & 0xFF);
            p[2] = (byte)((color >> 16) & 0xFF);
            p[3] = (byte)((color >> 24) & 0xFF);
        }

        private unsafe void DrawLine(byte* pix, int pitch, int w, int h, int x0, int y0, int x1, int y1, uint color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
                    SetPixel(pix, pitch, x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }
        #endregion

        public bool IsPixelTransparent(int x, int y) => false;
    }
}

