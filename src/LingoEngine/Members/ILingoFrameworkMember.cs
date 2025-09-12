using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.Members
{
    /// <summary>
    /// Lingo Framework Member Empty interface.
    /// </summary>
    public interface ILingoFrameworkMemberEmpty : ILingoFrameworkMember
    {

    }
    /// <summary>
    /// Lingo Framework Member With Texture interface.
    /// </summary>
    public interface ILingoFrameworkMemberWithTexture : ILingoFrameworkMember
    {
        /// <summary>
        /// The texture associated with this member, if any.
        /// </summary>
        IAbstTexture2D? TextureLingo { get; }
        IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor);
    }
    /// <summary>
    /// Lingo Framework Member interface.
    /// </summary>
    public interface ILingoFrameworkMember
    {
        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.text.loaded
        /// </summary>
        bool IsLoaded { get; }
        void CopyToClipboard();
        void Erase();
        void ImportFileInto();
        void PasteClipboardInto();
        void Preload();
        Task PreloadAsync();
        void ReleaseFromSprite(LingoSprite2D lingoSprite);
        void Unload();

        /// <summary>
        /// Determines whether the pixel at the given coordinates is fully transparent.
        /// Coordinates are relative to the member's top-left corner.
        /// </summary>
        /// <param name="x">X coordinate in pixels.</param>
        /// <param name="y">Y coordinate in pixels.</param>
        /// <returns><c>true</c> if the pixel is transparent; otherwise, <c>false</c>.</returns>
        bool IsPixelTransparent(int x, int y);
    }
}
