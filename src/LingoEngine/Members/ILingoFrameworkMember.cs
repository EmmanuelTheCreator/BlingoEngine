using LingoEngine.Sprites;

namespace LingoEngine.Members
{
    public interface ILingoFrameworkMemberEmpty : ILingoFrameworkMember
    {

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
        void ReleaseFromSprite(LingoSprite lingoSprite);
        void Unload();
    }
}
