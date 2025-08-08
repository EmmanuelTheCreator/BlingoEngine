using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using System;
using System.Linq;

namespace LingoEngine.LGodot.FilmLoops
{
    /// <summary>
    /// Godot framework implementation for film loop members.
    /// </summary>
    public class LingoGodotFilmLoopMember : ILingoFrameworkMemberFilmLoop, IDisposable
    {
        private LingoFilmLoopMember _member = null!;
        public bool IsLoaded { get; private set; }
        public byte[]? Media { get; set; }
        public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
        public bool Loop { get; set; } = true;
        public LingoPoint Offset { get; private set; }

        public LingoFilmLoopMember Member => _member;
        public LingoGodotFilmLoopMember()
        {
        }

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
            IsLoaded = false;
        }

        public void Erase()
        {
            Media = null;
            Unload();
        }

        public void ImportFileInto()
        {
            // Placeholder for future import logic
        }


        public ILingoTexture2D ComposeTexture(LingoSprite2D hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers)
        {
            var bounds = _member.GetBoundingBox();
            Offset = new LingoPoint(-bounds.Left, -bounds.Top);
            int width = (int)MathF.Ceiling(bounds.Width);
            int height = (int)MathF.Ceiling(bounds.Height);
            var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            image.Lock();
            foreach (var layer in layers)
            {
                if (layer.Member is not LingoMemberBitmap pic)
                    continue;

                var bmp = pic.Framework<LingoGodotMemberBitmap>();
                if (bmp.TextureGodot == null) continue;
                var srcTex = bmp.GetTextureForInk(layer.InkType, layer.BackColor);
                if (srcTex == null)
                    continue;
                var srcImg = srcTex.GetImage();
                int destW = (int)layer.Width;
                int destH = (int)layer.Height;

                if (Framing == LingoFilmLoopFraming.Scale)
                {
                    if (destW != srcImg.GetWidth() || destH != srcImg.GetHeight())
                        srcImg.Resize(destW, destH, Image.Interpolation.Bilinear);
                }
                else
                {
                    int cropW = Math.Min(destW, srcImg.GetWidth());
                    int cropH = Math.Min(destH, srcImg.GetHeight());
                    int cropX = (srcImg.GetWidth() - cropW) / 2;
                    int cropY = (srcImg.GetHeight() - cropH) / 2;
                    srcImg = srcImg.GetRegion(new Rect2I(cropX, cropY, cropW, cropH));
                    destW = cropW;
                    destH = cropH;
                }

                var srcCenter = new Vector2(destW / 2f, destH / 2f);
                var pos = new Vector2(layer.LocH + Offset.X, layer.LocV + Offset.Y);
                var scale = new Vector2(layer.FlipH ? -1 : 1, layer.FlipV ? -1 : 1);
                float skewX = Mathf.Tan(Mathf.DegToRad(layer.Skew));
                var skew = new Transform2D(new Vector2(1, 0), new Vector2(skewX, 1), Vector2.Zero);
                var transform = Transform2D.Identity;
                transform = transform.Translated(-srcCenter);
                transform = transform.Scaled(scale);
                transform = skew * transform;
                transform = transform.Rotated(Mathf.DegToRad(layer.Rotation));
                transform = transform.Translated(pos);
                BlendImage(image, srcImg, transform, Mathf.Clamp(layer.Blend / 100f, 0f, 1f));
            }
            image.Unlock();
            var tex = ImageTexture.CreateFromImage(image);
            var texture = new LingoGodotTexture2D(tex);
            return texture;
        }

        /// <summary>
        /// Blends <paramref name="src"/> onto <paramref name="dest"/> using the provided
        /// transform and opacity.
        /// TODO: consider extracting a shared abstraction for SDL and Godot to avoid
        /// duplicate pixel code and potentially cache small frames.
        /// </summary>
        private static void BlendImage(Image dest, Image src, Transform2D transform, float alpha)
        {
            src.Lock();
            var inv = transform.AffineInverse();
            Vector2[] pts =
            {
                transform * Vector2.Zero,
                transform * new Vector2(src.GetWidth(), 0),
                transform * new Vector2(src.GetWidth(), src.GetHeight()),
                transform * new Vector2(0, src.GetHeight())
            };
            int minX = (int)MathF.Floor(pts.Min(p => p.X));
            int maxX = (int)MathF.Ceiling(pts.Max(p => p.X));
            int minY = (int)MathF.Floor(pts.Min(p => p.Y));
            int maxY = (int)MathF.Ceiling(pts.Max(p => p.Y));

            for (int y = minY; y < maxY; y++)
            {
                if (y < 0 || y >= dest.GetHeight()) continue;
                for (int x = minX; x < maxX; x++)
                {
                    if (x < 0 || x >= dest.GetWidth()) continue;
                    var srcPos = inv * new Vector2(x + 0.5f, y + 0.5f);
                    int sx = (int)MathF.Floor(srcPos.X);
                    int sy = (int)MathF.Floor(srcPos.Y);
                    if (sx < 0 || sy < 0 || sx >= src.GetWidth() || sy >= src.GetHeight())
                        continue;
                    var c = src.GetPixel(sx, sy);
                    c.A *= alpha;
                    if (c.A <= 0f) continue;
                    var dst = dest.GetPixel(x, y);
                    float invA = 1f - c.A;
                    dest.SetPixel(x, y, new Color(
                        c.R + dst.R * invA,
                        c.G + dst.G * invA,
                        c.B + dst.B * invA,
                        c.A + dst.A * invA));
                }
            }
            src.Unlock();
        }
#if DEBUG
        public static void DebugToDisk(Image image, string filName)
        {
            var fn = "C:/temp/director/" + filName + ".png";
            if (File.Exists(fn))
                File.Delete(fn);
            var tex = ImageTexture.CreateFromImage(image);
            image.SavePng(fn);
        }
#endif

        public void Dispose()
        {
            Unload();
        }

        public LingoRect GetBoundingBox() => Member.GetBoundingBox();


        #region Clipboard

        public void CopyToClipboard()
        {
            //if (Texture is not LingoGodotTexture2D tex)
            //    return;
            //var img = tex.Texture.GetImage();
            //var bytes = img.SavePngToBuffer();
            //var base64 = Convert.ToBase64String(bytes);
            //DisplayServer.ClipboardSet(base64);
        }

        public void PasteClipboardInto()
        {
            var data = DisplayServer.ClipboardGet();
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                //var bytes = Convert.FromBase64String(data);
                //var image = new Image();
                //if (image.LoadPngFromBuffer(bytes) != Error.Ok)
                //    return;
                //Media = bytes;
                //Texture = new LingoGodotTexture2D(ImageTexture.CreateFromImage(image));
            }
            catch
            {
                // ignore malformed clipboard data
            }
        }
        #endregion
    }
}
