using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using System.Threading.Tasks;

namespace BlingoEngine.Bitmaps
{

    /// <summary>
    /// Represents a bitmap or picture cast member in a Director movie.
    /// Lingo: member("name").type = #bitmap or #picture
    /// </summary>
    public class BlingoMemberBitmap : BlingoMember, IBlingoMemberWithTexture
    {
        private readonly IBlingoFrameworkMemberBitmap _blingoFrameworkMemberPicture;

        /// <summary>
        /// Gets the framework object that implements the <see cref="IBlingoFrameworkMemberBitmap"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the framework object.</typeparam>
        /// <returns>Framework object implementing <see cref="IBlingoFrameworkMemberBitmap"/>.</returns>
        public T Framework<T>() where T : class, IBlingoFrameworkMemberBitmap => (T)_blingoFrameworkMemberPicture;

        /// <summary>
        /// Gets the raw image data (e.g., pixel data or encoded image format).
        /// This field is implementation-independent; renderers may interpret this as needed.
        /// </summary>
        public byte[]? ImageData => _blingoFrameworkMemberPicture.ImageData;

        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.picture.loaded
        /// </summary>
        public bool IsLoaded => _blingoFrameworkMemberPicture.IsLoaded;

        /// <summary>
        /// Gets the MIME type or encoding format of the image (e.g., "image/png", "image/jpeg").
        /// </summary>
        public string Format => _blingoFrameworkMemberPicture.Format;

        public IAbstTexture2D? TextureBlingo => _blingoFrameworkMemberPicture.TextureBlingo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlingoMemberBitmap"/> class.
        /// </summary>
        /// <param name="blingoFrameworkMemberPicture">The framework picture object.</param>
        /// <param name="numberInCast">The number of the member in the <see cref="BlingoCast"/>.</param>
        /// <param name="name">The name of the member.</param>
        public BlingoMemberBitmap(BlingoCast cast, IBlingoFrameworkMemberBitmap blingoFrameworkMemberPicture, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(blingoFrameworkMemberPicture, BlingoMemberType.Picture, cast, numberInCast, name, fileName, regPoint)
        {
            _blingoFrameworkMemberPicture = blingoFrameworkMemberPicture;
        }

        protected override BlingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_blingoFrameworkMemberPicture has to be retieved from the factory");
            //var clone = new BlingoMemberPicture(_cast, _blingoFrameworkMemberPicture, newNumber, Name);
            //return clone;
        }


        /// <summary>
        /// Preloads the picture into memory using <see cref="IBlingoFrameworkMemberBitmap.Preload"/>.
        /// Corresponds to: member.picture.preload
        /// </summary>
        public override void Preload() => _blingoFrameworkMemberPicture.Preload();
        public override Task PreloadAsync() => _blingoFrameworkMemberPicture.PreloadAsync();

        /// <summary>
        /// Unloads the picture from memory using <see cref="IBlingoFrameworkMemberBitmap.Unload"/>.
        /// Corresponds to: member.picture.unload
        /// </summary>
        public override void Unload() => _blingoFrameworkMemberPicture.Unload();

        /// <summary>
        /// Erases the picture using <see cref="IBlingoFrameworkMemberBitmap.Erase"/>.
        /// Corresponds to: member.picture.erase
        /// </summary>
        public override void Erase() => _blingoFrameworkMemberPicture.Erase();

        /// <summary>
        /// Imports a file into the picture. This is a placeholder for future external image loading functionality.
        /// </summary>
        public override void ImportFileInto()
        {
            // Future: Implement external image loading
        }

        /// <summary>
        /// Copies the picture to the clipboard.
        /// Corresponds to: member.picture.copy
        /// </summary>
        public override void CopyToClipBoard() => _blingoFrameworkMemberPicture.CopyToClipboard();

        /// <summary>
        /// Pastes the picture from the clipboard into the current picture.
        /// Corresponds to: member.picture.paste
        /// </summary>
        public override void PasteClipBoardInto() => _blingoFrameworkMemberPicture.PasteClipboardInto();

        public void SetImageData(byte[] bytes) => _blingoFrameworkMemberPicture.SetImageData(bytes);

        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
            => _blingoFrameworkMemberPicture.RenderToTexture(ink, transparentColor);
    }

}



