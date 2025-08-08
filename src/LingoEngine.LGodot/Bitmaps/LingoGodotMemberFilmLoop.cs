using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Sprites;
using System.Collections.Generic;
using System;
using LingoEngine.Members;

namespace LingoEngine.LGodot.Bitmaps
{
    /// <summary>
    /// Godot framework implementation for film loop members.
    /// </summary>
    public class LingoGodotMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
    {
        private LingoFilmLoopMember _member = null!;
        public bool IsLoaded { get; private set; }
        public byte[]? Media { get; set; }
        public ILingoTexture2D? Texture { get; set; }
        public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
        public bool Loop { get; set; } = true;

        public LingoGodotMemberFilmLoop()
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

        public void CopyToClipboard()
        {
            if (Texture is not LingoGodotTexture2D tex)
                return;
            var img = tex.Texture.GetImage();
            var bytes = img.SavePngToBuffer();
            var base64 = Convert.ToBase64String(bytes);
            DisplayServer.ClipboardSet(base64);
        }

        public void PasteClipboardInto()
        {
            var data = DisplayServer.ClipboardGet();
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                var bytes = Convert.FromBase64String(data);
                var image = new Image();
                if (image.LoadPngFromBuffer(bytes) != Error.Ok)
                    return;
                Media = bytes;
                Texture = new LingoGodotTexture2D(ImageTexture.CreateFromImage(image));
            }
            catch
            {
                // ignore malformed clipboard data
            }
        }

        public void ComposeTexture(LingoSprite2D hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers)
        {
            var image = Image.Create((int)hostSprite.Width, (int)hostSprite.Height, false, Image.Format.Rgba8);
            foreach (var layer in layers)
            {
                if (layer.Member is not LingoMemberBitmap pic)
                    continue;
                var bmp = pic.Framework<LingoGodotMemberBitmap>();
                var srcTex = bmp.TextureGodot;
                if (srcTex == null)
                    continue;

                var srcImg = srcTex.GetImage();
                int destW = (int)layer.Width;
                int destH = (int)layer.Height;

                if (Framing == LingoFilmLoopFraming.Scale)
                {
                    if (destW != srcImg.GetWidth() || destH != srcImg.GetHeight())
                        srcImg.Resize(destW, destH, Image.Interpolation.Bilinear);
                    int x = (int)(layer.LocH + hostSprite.Width / 2f - destW / 2f);
                    int y = (int)(layer.LocV + hostSprite.Height / 2f - destH / 2f);
                    image.BlendRect(srcImg, new Rect2I(0, 0, destW, destH), new Vector2I(x, y));
                }
                else
                {
                    int cropW = Math.Min(destW, srcImg.GetWidth());
                    int cropH = Math.Min(destH, srcImg.GetHeight());
                    int cropX = (srcImg.GetWidth() - cropW) / 2;
                    int cropY = (srcImg.GetHeight() - cropH) / 2;
                    var cropped = srcImg.GetRegion(new Rect2I(cropX, cropY, cropW, cropH));
                    int x = (int)(layer.LocH + hostSprite.Width / 2f - cropW / 2f);
                    int y = (int)(layer.LocV + hostSprite.Height / 2f - cropH / 2f);
                    image.BlendRect(cropped, new Rect2I(0, 0, cropW, cropH), new Vector2I(x, y));
                }
            }
            var tex = ImageTexture.CreateFromImage(image);
            Texture = new LingoGodotTexture2D(tex);
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
