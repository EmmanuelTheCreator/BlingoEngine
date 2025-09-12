using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Members;
using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Helpers;
using System.Threading.Tasks;

namespace LingoEngine.LGodot.FilmLoops
{
    /// <summary>
    /// Godot framework implementation for film loop members.
    /// </summary>
    public class LingoGodotFilmLoopMember : ILingoFrameworkMemberFilmLoop, IDisposable
    {
        private LingoFilmLoopMember _member = null!;
        private AbstGodotTexture2D? _texture;
        public IAbstTexture2D? TextureLingo => _texture;

        public bool IsLoaded { get; private set; }
        public byte[]? Media { get; set; }
        public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
        public bool Loop { get; set; } = true;
        public APoint Offset { get; private set; }

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
            if (IsLoaded)
                return;
            IsLoaded = true;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
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
        public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
        {
            return _texture;
        }
        public IAbstTexture2D ComposeTexture(ILingoSprite2DLight hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers, int frame)
        {
            var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
            Offset = prep.Offset;
            int width = prep.Width;
            int height = prep.Height;
            var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            var i = 0;
            foreach (var info in prep.Layers)
            {
                i++;
                Image? srcImg = null;
                if (info.Sprite2D.Member is ILingoMemberWithTexture memberWithTexture)
                {
                    var textureG = memberWithTexture.RenderToTexture(info.Ink, info.BackColor) as AbstGodotTexture2D;
                    if (textureG != null)
                        srcImg = textureG.Texture.GetImage();
                }
                else
                {
                    if (info.Sprite2D.Texture == null) continue;
                    srcImg = ((AbstGodotTexture2D)info.Sprite2D.Texture).Texture.GetImage();
                }
                if (srcImg == null)
                    continue;

                //DebugToDisk(srcImg, $"filmloop_{frame}/{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");

                srcImg = srcImg.GetRegion(new Rect2I(info.SrcX, info.SrcY, info.SrcW, info.SrcH));
                if (info.DestW != info.SrcW || info.DestH != info.SrcH)
                    srcImg.Resize(info.DestW, info.DestH, Image.Interpolation.Bilinear);
                var m = info.Transform.Matrix;
                var transform = new Transform2D(
                    new Vector2(m.M11, m.M12),
                    new Vector2(m.M21, m.M22),
                    new Vector2(m.M31, m.M32));
                srcImg.BlendImageTo(image, transform, info.Alpha);

            }

            #region OLD
            //foreach (var layer in layers)
            //{
            //    if (layer.Texture is null)
            //        continue;
            //    //var nestedPlayer = layer.GetFilmLoopPlayer();
            //    //if (nestedPlayer?.Texture is not LingoGodotTexture2D nestedTex)
            //    //    continue;
            //    //var srcTex = nestedTex.Texture;
            //    var srcTex = ((LingoGodotTexture2D)layer.Texture).Texture;
            //    var srcImg = srcTex.GetImage();
            //    int srcW = srcTex.GetWidth();
            //    int srcH = srcTex.GetHeight();
            //    int destW = (int)layer.Width;
            //    int destH = (int)layer.Height;
            //    int srcX = 0;
            //    int srcY = 0;
            //    if (Framing != LingoFilmLoopFraming.Scale)
            //    {
            //        int cropW = Math.Min(destW, srcW);
            //        int cropH = Math.Min(destH, srcH);
            //        srcX = (srcW - cropW) / 2;
            //        srcY = (srcH - cropH) / 2;
            //        srcW = cropW;
            //        srcH = cropH;
            //        destW = cropW;
            //        destH = cropH;
            //    }
            //    srcImg = srcImg.GetRegion(new Rect2I(srcX, srcY, srcW, srcH));
            //    if (destW != srcW || destH != srcH)
            //        srcImg.Resize(destW, destH, Image.Interpolation.Bilinear);

            //    var srcCenter = new LingoPoint(destW / 2f, destH / 2f);
            //    var pos = new LingoPoint(layer.LocH + Offset.X, layer.LocV + Offset.Y);
            //    var scale = new LingoPoint(layer.FlipH ? -1 : 1, layer.FlipV ? -1 : 1);
            //    var tform = LingoTransform2D.Identity
            //        .Translated(-srcCenter.X, -srcCenter.Y)
            //        .Scaled(scale.X, scale.Y)
            //        .Skewed(layer.Skew)
            //        .Rotated(layer.Rotation)
            //        .Translated(pos.X, pos.Y);
            //    var m2 = tform.Matrix;
            //    var transform2D = new Transform2D(
            //        new Vector2(m2.M11, m2.M12),
            //        new Vector2(m2.M21, m2.M22),
            //        new Vector2(m2.M31, m2.M32));
            //    srcImg.BlendImageTo(image, transform2D, Math.Clamp(layer.Blend / 100f, 0f, 1f));
            //} 
            #endregion

            //DebugToDisk(image, $"filmloop_{frame}_{_member.Name}_{hostSprite.Name}");
            var tex = ImageTexture.CreateFromImage(image);
            _texture = new AbstGodotTexture2D(tex);
            _texture.Name = $"Filmloop_{frame}_Member_" + _member.Name;
            return _texture;
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

        public ARect GetBoundingBox() => Member.GetBoundingBox();
        public bool IsPixelTransparent(int x, int y) => false;

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
