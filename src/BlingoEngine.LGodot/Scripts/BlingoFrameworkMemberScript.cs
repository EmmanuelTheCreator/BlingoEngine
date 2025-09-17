using BlingoEngine.Members;
using BlingoEngine.Scripts;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Scripts
{
    internal class BlingoFrameworkMemberScript : IBlingoFrameworkMemberScript
    {
        public bool IsLoaded => true;
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
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
        public bool IsPixelTransparent(int x, int y) => false;
    }
}


