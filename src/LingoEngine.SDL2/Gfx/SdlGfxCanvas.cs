using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Primitives;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Pictures;
using LingoEngine.Styles;
using LingoEngine.Texts;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxCanvas : SdlGfxComponent, ILingoFrameworkGfxCanvas, IDisposable
    {
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        private readonly ILingoFontManager _fontManager;
        private readonly int _width;
        private readonly int _height;
        private nint _texture;
        public object FrameworkNode => this;
        public nint Texture => _texture;

        public bool Pixilated { get; set; }

        public SdlGfxCanvas(SdlGfxFactory factory, ILingoFontManager fontManager, int width, int height) : base(factory)
        {
            _fontManager = fontManager;
            _width = width;
            _height = height;
            _texture = SDL.SDL_CreateTexture(ComponentContext.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
            Width = width;
            Height = height;
        }

        private void UseTexture(Action draw)
        {
            var prev = SDL.SDL_GetRenderTarget(ComponentContext.Renderer);
            SDL.SDL_SetRenderTarget(ComponentContext.Renderer, _texture);
            draw();
            SDL.SDL_SetRenderTarget(ComponentContext.Renderer, prev);
        }

        public void Clear(LingoColor color)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderClear(ComponentContext.Renderer);
            });
        }

        public void SetPixel(LingoPoint point, LingoColor color)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderDrawPointF(ComponentContext.Renderer, point.X, point.Y);
            });
        }

        public void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, start.X, start.Y, end.X, end.Y);
            });
        }

        public void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                SDL.SDL_Rect r = new SDL.SDL_Rect
                {
                    x = (int)rect.Left,
                    y = (int)rect.Top,
                    w = (int)rect.Width,
                    h = (int)rect.Height
                };
                if (filled)
                    SDL.SDL_RenderFillRect(ComponentContext.Renderer, ref r);
                else
                    SDL.SDL_RenderDrawRect(ComponentContext.Renderer, ref r);
            });
        }

        public void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                int segs = (int)(radius * 6);
                double step = (Math.PI * 2) / segs;
                float prevX = center.X + radius;
                float prevY = center.Y;
                for (int i = 1; i <= segs; i++)
                {
                    double angle = step * i;
                    float x = center.X + (float)(radius * Math.Cos(angle));
                    float y = center.Y + (float)(radius * Math.Sin(angle));
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, prevX, prevY, x, y);
                    if (filled)
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, center.X, center.Y, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
        }

        public void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                double startRad = startDeg * Math.PI / 180.0;
                double endRad = endDeg * Math.PI / 180.0;
                double step = (endRad - startRad) / segments;
                float prevX = center.X + (float)(radius * Math.Cos(startRad));
                float prevY = center.Y + (float)(radius * Math.Sin(startRad));
                for (int i = 1; i <= segments; i++)
                {
                    double ang = startRad + i * step;
                    float x = center.X + (float)(radius * Math.Cos(ang));
                    float y = center.Y + (float)(radius * Math.Sin(ang));
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, prevX, prevY, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
        }

        public void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1)
        {
            if (points.Count < 2) return;
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, color.R, color.G, color.B, 255);
                for (int i = 0; i < points.Count - 1; i++)
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, points[^1].X, points[^1].Y, points[0].X, points[0].Y);
                if (filled)
                {
                    var p0 = points[0];
                    for (int i = 1; i < points.Count - 1; i++)
                    {
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, p0.X, p0.Y, points[i].X, points[i].Y);
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, p0.X, p0.Y, points[i + 1].X, points[i + 1].Y);
                    }
                }
            });
        }

        public void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12, int width = -1, LingoTextAlignment alignment = default)
        {
            UseTexture(() =>
            {
                var path = _fontManager.Get<string>(font ?? string.Empty);
                if (string.IsNullOrEmpty(path)) return;
                nint fnt = SDL_ttf.TTF_OpenFont(path, fontSize);
                if (fnt == nint.Zero) return;
                SDL.SDL_Color c = new SDL.SDL_Color { r = color?.R ?? 0, g = color?.G ?? 0, b = color?.B ?? 0, a = 255 };

                string RenderLine(string line)
                {
                    if (width >= 0)
                    {
                        while (line.Length > 0 && SDL_ttf.TTF_SizeUTF8(fnt, line, out int w, out _) == 0 && w > width)
                        {
                            line = line.Substring(0, line.Length - 1);
                        }
                    }
                    return line;
                }

                string[] lines = text.Split('\n');
                List<(nint surf, int w, int h)> surfaces = new();
                foreach (var ln in lines)
                {
                    var line = RenderLine(ln);
                    nint s = SDL_ttf.TTF_RenderUTF8_Blended(fnt, line, c);
                    if (s == nint.Zero) continue;
                    var sur = SDL.PtrToStructure<SDL.SDL_Surface>(s);
                    surfaces.Add((s, sur.w, sur.h));
                }

                int y = (int)position.Y;
                foreach (var (s, w, h) in surfaces)
                {
                    nint tex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, s);
                    if (tex != nint.Zero)
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)position.X, y = y, w = w, h = h };
                        SDL.SDL_RenderCopy(ComponentContext.Renderer, tex, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(tex);
                    }
                    SDL.SDL_FreeSurface(s);
                    y += h;
                }

                SDL_ttf.TTF_CloseFont(fnt);
            });
        }

        public void DrawPicture(byte[] data, int width, int height, LingoPoint position, LingoPixelFormat format)
        {
            UseTexture(() =>
            {
                format.GetMasks(out uint rmask, out uint gmask, out uint bmask, out uint amask, out int bpp);
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                nint surf = SDL.SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), width, height, bpp, width * (bpp / 8), rmask, gmask, bmask, amask);
                if (surf == nint.Zero) { handle.Free(); return; }
                nint tex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, surf);
                if (tex != nint.Zero)
                {
                    SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)position.X, y = (int)position.Y, w = width, h = height };
                    SDL.SDL_RenderCopy(ComponentContext.Renderer, tex, nint.Zero, ref dst);
                    SDL.SDL_DestroyTexture(tex);
                }
                SDL.SDL_FreeSurface(surf);
                handle.Free();
            });
        }
        public void DrawPicture(ILingoImageTexture texture, int width, int height, LingoPoint position)
        {
            UseTexture(() =>
            {
                if (texture is SdlImageTexture img)
                {
                    nint tex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.SurfaceId);
                    if (tex != nint.Zero)
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect
                        {
                            x = (int)position.X,
                            y = (int)position.Y,
                            w = width,
                            h = height
                        };
                        SDL.SDL_RenderCopy(ComponentContext.Renderer, tex, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(tex);
                    }
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
        }

        public override LingoSDLRenderResult Render(LingoSDLRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ComponentContext.Renderer = context.Renderer;

            var screenPos = context.Origin + new Vector2(X, Y);
            ImGui.SetCursorScreenPos(screenPos);
            ImGui.PushID(Name);
            ImGui.Image(_texture, new Vector2(Width, Height));
            ImGui.PopID();

            return LingoSDLRenderResult.RequireRender();
        }

    }
}
