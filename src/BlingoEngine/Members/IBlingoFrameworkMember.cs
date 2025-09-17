using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.Members
{
    /// <summary>
    /// Lingo Framework Member Empty interface.
    /// </summary>
    public interface IBlingoFrameworkMemberEmpty : IBlingoFrameworkMember
    {

    }
    /// <summary>
    /// Lingo Framework Member With Texture interface.
    /// </summary>
    public interface IBlingoFrameworkMemberWithTexture : IBlingoFrameworkMember
    {
        /// <summary>
        /// The texture associated with this member, if any.
        /// </summary>
        IAbstTexture2D? TextureBlingo { get; }
        IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor);
    }
    /// <summary>
    /// Lingo Framework Member interface.
    /// </summary>
    public interface IBlingoFrameworkMember
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
        void ReleaseFromSprite(BlingoSprite2D blingoSprite);
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

