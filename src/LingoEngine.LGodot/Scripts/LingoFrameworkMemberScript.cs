using LingoEngine.Members;
using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.LGodot.Scripts
{
    internal class LingoFrameworkMemberScript : ILingoFrameworkMemberScript
    {
        public bool IsLoaded => true;
        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
        public void CopyToClipboard()
        {
        }

        public void Erase()
        {
        }

        public void ImportFileInto()
        {
        }

        public void PasteClipboardInto()
        {
        }

        public void Preload()
        {
        }

        public void Unload()
        {
        }
        public bool IsPixelTransparent(int x, int y) => false;
    }
}

