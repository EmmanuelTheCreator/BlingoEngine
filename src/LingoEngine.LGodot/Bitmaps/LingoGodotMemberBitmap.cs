using Godot;
using AbstUI.Primitives;
using AbstUI.Tools;
using LingoEngine.Bitmaps;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using LingoEngine.Tools;
using Microsoft.Extensions.Logging;
using AbstUI.LGodot.Helpers;
using AbstUI.LGodot.Bitmaps;
using System.Threading.Tasks;

namespace LingoEngine.LGodot.Bitmaps
{
    public class LingoGodotMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
    {
        private LingoMemberBitmap _lingoMemberPicture;
        private AbstGodotTexture2D? _texture;
        private Image? _image;
        private readonly ILogger _logger;
        private readonly Dictionary<LingoInkType, AbstGodotTexture2D> _inkCache = new();

        public AbstGodotTexture2D? TextureImage => _texture;
        public IAbstTexture2D? TextureLingo
        {
            get
            {
                if (!IsLoaded) Preload();
                return _texture;
            }
        }

        public byte[]? ImageData { get; private set; }

        public bool IsLoaded { get; private set; }
        /// <summary>
        /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
        /// </summary>
        public string Format { get; private set; } = "image/unknown";

        public int Width { get; private set; }

        public int Height { get; private set; }


#pragma warning disable CS8618
        public LingoGodotMemberBitmap(ILogger<LingoGodotMemberBitmap> logger)
#pragma warning restore CS8618 
        {
            _logger = logger;
        }

        internal void Init(LingoMemberBitmap lingoInstance)
        {
            _lingoMemberPicture = lingoInstance;
            CreateTexture();
        }

        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }


        private void UpdateImageData(Image image)
        {
            Width = image.GetWidth();
            Height = image.GetHeight();
            ImageData = image.GetData();

            Format = MimeHelper.GetMimeType(_lingoMemberPicture.FileName);
            _lingoMemberPicture.Size = ImageData.Length;
            _lingoMemberPicture.Width = Width;
            _lingoMemberPicture.Height = Height;
        }
        public void Erase()
        {
            Unload();
            _image?.Dispose();
            ImageData = null;
            IsLoaded = false;
        }


        public void Preload()
        {
            if (IsLoaded) return;
            if (_image == null) CreateTexture();
            IsLoaded = true;
            if (_image == null || _image.IsEmpty())
                return;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            ClearCache();
            _texture?.Texture.Dispose();
            _texture = null;
            IsLoaded = false;
        }

        public void Dispose()
        {
            ClearCache();
            _image?.Dispose();
            _texture?.Texture.Dispose();
        }

        public void ImportFileInto()
        {
        }
        /// <summary>
        /// Creates an ImageTexture from the ImageData byte array.
        /// </summary>
        public void CreateTexture()
        {
            // Create a new Image object
            _image = new Image();
            string filePath = GodotHelper.EnsureGodotUrl(_lingoMemberPicture.FileName);

            // Load the image from the byte array (assuming PNG/JPEG format)
            if (!ResourceLoader.Exists(filePath))
            {
                _logger.LogWarning($"MemberBitmap not found: {filePath}");
                return;
            }

            var texture = ResourceLoader.Load<Texture2D>(filePath);

            if (texture == null)
            {
                _logger.LogError($"Failed to load Texture2D for MemberBitmap at {filePath}");
                return;
            }

            Image image;
            try
            {
                image = texture.GetImage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Texture.GetImage() failed at {filePath} for MemberBitmap. Is 'Keep -> Image' enabled in import settings?");
                return;
            }

            if (image.IsEmpty())
            {
                _logger.LogError($"Image is empty: {filePath}");
                return;
            }
            _image = image;
            var imageTexture = ImageTexture.CreateFromImage(_image);
            if (imageTexture == null) return;
            _texture = new AbstGodotTexture2D(imageTexture);

            UpdateImageData(_image);
        }
        public Image GetImageCopy()
        {
            if (_image == null)
                throw new InvalidOperationException("Image not loaded.");

            return _image.Duplicate() as Image
                ?? throw new InvalidOperationException("Failed to duplicate image.");
        }
        public void ApplyImage(Image editedImage)
        {
            _image?.Dispose(); // Dispose old image
            _image = editedImage.Duplicate() as Image ?? throw new InvalidOperationException("Failed to copy edited image.");
            _texture?.Texture.Dispose();

            var imageTexture = ImageTexture.CreateFromImage(_image);
            UpdateImageData(_image); // Also updates width, height, and member size
            _texture = new AbstGodotTexture2D(imageTexture);
            // Update the member's image data directly
            _lingoMemberPicture.SetImageData(_image.GetData());
        }
        public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => GetTextureForInk(ink, transparentColor);
        public IAbstTexture2D? GetTextureForInk(LingoInkType ink, AColor backColor)
        {
            if (!InkPreRenderer.CanHandle(ink) || _image == null)
                return null;

            var inkKey = InkPreRenderer.GetInkCacheKey(ink);
            if (_inkCache.TryGetValue(inkKey, out var tex))
                return tex; //.Clone();

            var img = GetImageCopy();
            if (img.IsCompressed())
                img.Decompress();
            img.Convert(Image.Format.Rgba8);
            var data = InkPreRenderer.Apply(img.GetData(), ink, backColor);
            var newImg = Image.CreateFromData(img.GetWidth(), img.GetHeight(), false, Image.Format.Rgba8, data);
            var tex1 = ImageTexture.CreateFromImage(newImg);
            var newTexture = new AbstGodotTexture2D(tex1);
            _texture = newTexture;
            newImg.Dispose();
            _inkCache[inkKey] = newTexture;
            return newTexture; //.Clone();
        }

        public bool IsPixelTransparent(int x, int y)
        {
            if (_image == null)
                return false;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return true;

            var color = _image.GetPixel(x, y);
            return color.A <= 0.001f;
        }

        public void SetImageData(byte[] bytes) => ImageData = bytes;

        private void ClearCache()
        {
            foreach (var tex in _inkCache.Values)
                tex?.Texture.Dispose();
            _inkCache.Clear();
        }

        #region Clipboard
        public void CopyToClipboard()
        {
            if (_image == null) CreateTexture();
            if (ImageData == null) return;
            var base64 = Convert.ToBase64String(ImageData);
            DisplayServer.ClipboardSet("data:" + Format + ";base64," + base64);
        }
        public void PasteClipboardInto()
        {
            _image = DisplayServer.ClipboardGetImage();
            UpdateImageData(_image);
        }

        #endregion
    }
}
