using LingoEngine.Members;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.LGodot.Core
{
    public class LingoFrameworkMemberEmpty : ILingoFrameworkMemberEmpty
    {
        public bool IsLoaded => true;

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
            if (IsLoaded)
                return;
        }

        public Task PreloadAsync() => Task.CompletedTask;

        public void Unload()
        {
        }
        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
        public bool IsPixelTransparent(int x, int y) => false;
    }
}
