using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

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
            //var image = Image.CreateEmpty((int)hostSprite.Width, (int)hostSprite.Height, false, Image.Format.Rgba8);
            var image = Image.CreateEmpty(300, 300, false, Image.Format.Rgba8);
            foreach (var layer in layers)
            {
                if (layer.Member is not LingoMemberBitmap pic)
                    // TODO : add text and in SDL too
                    continue;
                
                var bmp = pic.Framework<LingoGodotMemberBitmap>();
                //var srcTex = bmp.TextureGodot;
                if (bmp.TextureGodot == null) continue;
                var srcTex = bmp.GetTextureForInk(layer.InkType, layer.BackColor);
                if (srcTex == null)
                    continue;
                var srcImg = srcTex.GetImage();
                //DebugToDisk(srcImg, "Layer_"+layer.SpriteNum);
                //image.BlitRect(srcImg, new Rect2I(0, 0, srcImg.GetWidth(), srcImg.GetHeight()), new Vector2I(3, 3));
                //image.BlendRect(srcImg, new Rect2I(0, 0, bmp.Width, bmp.Height), new Vector2I(3, 3));
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
                    image.BlendRect(srcImg, new Rect2I(0, 0, cropW, cropH), new Vector2I(x, y));
                }
            }
            //DebugToDisk(image,"Complete");
            var tex = ImageTexture.CreateFromImage(image);
            var texture = new LingoGodotTexture2D(tex);
            return texture;
        }
#if DEBUG
        public static void DebugToDisk(Image image, string filName)
        {
            var fn = "C:/temp/director/"+ filName+".png";
            if ( File.Exists(fn))
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
