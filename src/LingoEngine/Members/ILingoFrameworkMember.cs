using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Members
{
    public interface ILingoFrameworkMemberEmpty : ILingoFrameworkMember
    {

    }
    public interface ILingoFrameworkMemberWithTexture : ILingoFrameworkMember
    {
        /// <summary>
        /// The texture associated with this member, if any.
        /// </summary>
        IAbstTexture2D? TextureLingo { get; }
        IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor);
    }
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
        void ReleaseFromSprite(LingoSprite2D lingoSprite);
        void Unload();

        /// <summary>
        /// Determines whether the pixel at the given coordinates is fully transparent.
        /// Coordinates are relative to the member's top-left corner.
        /// </summary>
        /// <param name="x">X coordinate in pixels.</param>
        /// <param name="y">Y coordinate in pixels.</param>
        /// <returns><c>true</c> if the pixel is transparent; otherwise, <c>false</c>.</returns>
#if NET48
        bool IsPixelTransparent(int x, int y);
#else
        bool IsPixelTransparent(int x, int y) => false;
#endif
    }
}
