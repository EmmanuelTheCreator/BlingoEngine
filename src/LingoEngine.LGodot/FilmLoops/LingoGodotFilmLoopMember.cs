using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
            Offset = prep.Offset;
            int width = prep.Width;
            int height = prep.Height;
            var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
             var i = 0;
            foreach (var info in prep.Layers)

            {
                if (info.Bitmap is not LingoGodotMemberBitmap bmp)
                    continue;
                if (bmp.TextureGodot == null) continue;
                var srcTex = bmp.GetTextureForInk(info.Ink, info.BackColor);
                if (srcTex == null)
                    continue;
                var srcImg = srcTex.GetImage();

                srcImg = srcImg.GetRegion(new Rect2I(info.SrcX, info.SrcY, info.SrcW, info.SrcH));
                if (info.DestW != info.SrcW || info.DestH != info.SrcH)
                    srcImg.Resize(info.DestW, info.DestH, Image.Interpolation.Bilinear);

                var m = info.Transform.Matrix;
                var transform = new Transform2D(
                    new Vector2(m.M11, m.M12),
                    new Vector2(m.M21, m.M22),
                    new Vector2(m.M31, m.M32));
                BlendImage(image, srcImg, transform, info.Alpha);
                DebugToDisk(srcImg, $"filmloop_{i}_{pic.Name}");

            }
            DebugToDisk(image, $"filmloop_{_member.Name}_{hostSprite.Name}");
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

            int destWidth = dest.GetWidth();
            int destHeight = dest.GetHeight();
            int srcWidth = src.GetWidth();
            int srcHeight = src.GetHeight();
            var destData = dest.GetData();
            var srcData = src.GetData();
            var destSpan = destData.AsSpan();
            var srcSpan = srcData.AsSpan();
            int destPitch = destWidth * 4;
            int srcPitch = srcWidth * 4;

            Parallel.For(minY, maxY, y =>
            {
                if (y < 0 || y >= destHeight) return;
                int destRow = y * destPitch;
                for (int x = minX; x < maxX; x++)
                {
                    if (x < 0 || x >= destWidth) continue;
                    var srcPos = inv * new Vector2(x + 0.5f, y + 0.5f);
                    int sx = (int)MathF.Floor(srcPos.X);
                    int sy = (int)MathF.Floor(srcPos.Y);
                    if (sx < 0 || sy < 0 || sx >= srcWidth || sy >= srcHeight)
                        continue;
                    int srcIndex = sy * srcPitch + sx * 4;
                    float a = srcSpan[srcIndex + 3] / 255f * alpha;
                    if (a <= 0f) continue;
                    int destIndex = destRow + x * 4;
                    float invA = 1f - a;
                    destSpan[destIndex] = (byte)(srcSpan[srcIndex] * a + destSpan[destIndex] * invA);
                    destSpan[destIndex + 1] = (byte)(srcSpan[srcIndex + 1] * a + destSpan[destIndex + 1] * invA);
                    destSpan[destIndex + 2] = (byte)(srcSpan[srcIndex + 2] * a + destSpan[destIndex + 2] * invA);
                    destSpan[destIndex + 3] = (byte)(srcSpan[srcIndex + 3] * a + destSpan[destIndex + 3] * invA);
                }
            });

            dest.SetData(destWidth, destHeight, false, Image.Format.Rgba8, destData);
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
