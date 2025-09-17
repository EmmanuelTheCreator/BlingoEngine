using BlingoEngine.Members;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.Transitions
{
    public class BlingoTransitionMemberFW : IBlingoFrameworkMember
    {
        public BlingoTransitionMemberFW()
        {
        }

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

        public bool IsPixelTransparent(int x, int y) => false;

        public void PasteClipboardInto()
        {

        }

        public void Preload()
        {
            if (IsLoaded)
                return;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void ReleaseFromSprite(BlingoSprite2D blingoSprite)
        {

        }

        public void Unload()
        {

        }
    }
}

