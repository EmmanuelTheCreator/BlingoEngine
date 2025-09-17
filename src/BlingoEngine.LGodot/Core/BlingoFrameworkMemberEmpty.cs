using BlingoEngine.Members;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Core
{
    public class BlingoFrameworkMemberEmpty : IBlingoFrameworkMemberEmpty
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
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
        public bool IsPixelTransparent(int x, int y) => false;
    }
}

